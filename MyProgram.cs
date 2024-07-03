using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;

namespace MultiBoid {
	public class MyProgram {
		int boidCount = 100;
		int threadCount = 1;

		static public MyProgram? myInstance;
		public static void MyMain(MainWindow window) {
			myInstance = new MyProgram(window);
		}
		public MyProgram(MainWindow window) {
			this.window = window;
			Start();
		}

		MainWindow window;
		List<Vector> boidPos = new List<Vector>(); 
		List<Vector> boidVel = new List<Vector>();
		
		Stopwatch stopwatch = new Stopwatch();

		void Start() {
			Random rand = new Random();
			for (int i = 0; i < boidCount; i++) {
				boidPos.Add(new Vector(rand.Next((int)window.Width), rand.Next((int)window.Height)));
				boidVel.Add(new Vector(rand.Next(50), rand.Next(50)));
			}
			stopwatch.Start();
			Console.WriteLine("Running simulation...");
			Console.WriteLine("Measuring average time per tick.");
			TestBracket((100, 1200, 500), (0, 5, 2), 90);
			Update();
		}

		public void RecieveControl(object? sender, EventArgs args) {
			Update();
		}

		int frame = 0;
		int frameRatio = 1;

		void Update() {
			while (true) {
				float dT = fixTickrate(30);
				Tick(dT);

				frame++;
				if (frame >= frameRatio) {
					frame = 0;
					render();
					break;
				}
			}
		}

		void TestBracket((int lB, int uB, int inc) BoidRange, (int lB, int uB, int inc) ThreadRange, int ticks) {
			//(int, int, int) => lowerBound, upperBound (exclusive), increment

			var data = "";
			for (int i = BoidRange.lB; i < BoidRange.uB; i += BoidRange.inc) {
				data += "\nBoids: " + i + "; Threads: delay (ms):\n";
				Console.Write("\n  Boids: " + i + "\n");
				for (int j = ThreadRange.lB; j < ThreadRange.uB; j += ThreadRange.inc) {
					data += j + ": " + Test(i, j, ticks) + ", ";
					Console.Write(j + " threads: " + Math.Round(Test(i, j, ticks), 2) + "ms, ");
				}
			}

			//Write data to file
			StreamWriter SW = new StreamWriter("MultiBoidData.txt");
			SW.Write(data);
			SW.Dispose();
			throw new Exception("Finished Test");//Better way to exit?

			double Test(int BoidCount, int ThreadCount, int ticks) {
				//Initilize
				boidCount = BoidCount;
				threadCount = ThreadCount;
				boidPos.Clear();
				boidVel.Clear();
				Random rand = new Random();
				for (int i = 0; i < BoidCount; i++) {
					boidPos.Add(new Vector(rand.Next((int)window.Width), rand.Next((int)window.Height)));
					boidVel.Add(new Vector(rand.Next(50), rand.Next(50)));
				}

				//Warmup
				for (int i = 0; i < ticks; i++)
					Tick(0.03f);

				//Main Test
				var t0 = DateTime.Now;
				for (int i = 0; i < ticks; i++)
					Tick(0.03f);
				return (DateTime.Now - t0).TotalMilliseconds / ticks;
			}
		}

		void Tick(float dT) {
			//Debug.WriteLine(String.Format("Tickrate: {0}; Delay: {1}", 1/dT, dT*1000));
			var outVel = new List<Vector>(new Vector[boidVel.Count]);
			int atIndex = 0;
			int threadSize = threadCount >= 0 ? boidCount / (threadCount + 1) : 0;
			List<Thread> workerThreads = new List<Thread>();

			//Start worker threads
			//var t0 = DateTime.Now;
			for (int i = 0; i < threadCount; i++) {
				int a = atIndex, b = (atIndex + threadSize);
				workerThreads.Add(new Thread(new ThreadStart(() => Phys(a, b))));
				workerThreads[i].Name = "Worker thread " + i;
				workerThreads[i].Start();
				atIndex += threadSize;
			}
			//Debug.WriteLine("ThreadCreate: " + (DateTime.Now - t0).TotalMilliseconds);

			//Main thread works too
			Phys(atIndex, outVel.Count);

			//Await all threads
			//var t = DateTime.Now;
			foreach (var thread in workerThreads)
				thread.Join();
			//Debug.WriteLine("ThreadWait: " + (DateTime.Now - t).TotalMilliseconds);
			//Debug.WriteLine("TickWait: " + (DateTime.Now - t0).TotalMilliseconds);


			//Replace boidVel with outVel

			boidVel = outVel;

			for (int i = 0; i < boidPos.Count; i++) {
				//Update position
				boidPos[i] += boidVel[i] * dT;
				boidPos[i] = new Vector(boidPos[i].x % (float)window.Width, boidPos[i].y % (float)window.Height);
			}


			void Phys(int start, int end) {
				for (int i = start; i < end; i++) {
					float C = 0.001f;
					float S = 1f;
					float A = 0.01f;
					float B = 5f; //Barrier force
					Vector myVel = new Vector(boidVel[i]);

					for (int j = 0; j < boidPos.Count; j++) {
						if (i == j)
							continue;
						Vector dir = (boidPos[j] - boidPos[i]);

						myVel = myVel.Rotate((dir.Angle - myVel.Angle) * C * dT);

						if (dir.Length <= 10)
							myVel = myVel.Rotate((-dir.Angle - myVel.Angle) * S * dT / dir.Length);

						myVel = myVel.Rotate((boidVel[j].Angle - myVel.Angle) * A * dT / dir.Length);
					}

					//Update velocity
					outVel[i] = myVel;
				}
			}
		}

		public bool render() {
			Canvas canvas = new Canvas();

			foreach (var item in boidPos) {
				draw(item);
			}

			window.Content = canvas;
			window.Show();
			return true;


			void draw(Vector position) {
				Ellipse myEllipse = new Ellipse();
				myEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

				myEllipse.Width = 5;
				myEllipse.Height = myEllipse.Width;

				Canvas.SetLeft(myEllipse, position.x);
				Canvas.SetTop(myEllipse, position.y);
				canvas.Children.Add(myEllipse);
			}
		}

		float fixTickrate(int targetTickrate) {
			long targetDelay = 1000 / ((targetTickrate != 0) ? targetTickrate : 1);//Turn 1/s into ms
			long fixDelay = targetDelay - stopwatch.ElapsedMilliseconds;

			if (targetTickrate > 0 && fixDelay > 0)
				Thread.Sleep((int)fixDelay);

			float dT = stopwatch.ElapsedMilliseconds / 1000f;
			stopwatch.Restart();

			return dT;
		}
	}
}


/* Tests:

Test(BoidCount, ThreadCount):
	Instantiate
	Run for x ticks (warmup)
	Start recording
	Run for y ticks
	Save average tickrate

TestBracket(BoidRange, ThreadRange):
	For BoidRange, ThreadRange
		Test()
	Export data to .txt

*/

/* To Do:

V Implement Multithreading

Proper boid simulation
	Revamp C/S/A calcs

X Visualize delays
	V Export values to excel somehow
	From Creating Threads
	From Waiting on Threads
	From main thread computation
	From rendering

V Automatic test
	Range of thread count
	Range of boid Count

//--Low Priority--//

Rendering Optimizations
	Re-use canvas for rendering

Memory Leak?

Simulation Optimizations
	Quad tree / limit visual range

*/