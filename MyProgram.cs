using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace MultiBoid {
	public class MyProgram {
		int fieldSize = 1000;
		int boidCount = 100;
		int threadCount = 1;

		List<Vector> boidPos = new List<Vector>(); 
		List<Vector> boidVel = new List<Vector>();
		
		Stopwatch stopwatch = new Stopwatch();


		static void Main(string[] args) {
			Console.WriteLine("Hello, World!");
			var pg = new MyProgram();
			pg.Start();
		}

		public void Start() {
			Random rand = new Random();
			for (int i = 0; i < boidCount; i++) {
				boidPos.Add(new Vector(rand.Next(fieldSize), rand.Next(fieldSize)));
				boidVel.Add(new Vector(rand.Next(50), rand.Next(50)));
			}
			stopwatch.Start();

			TestBracket((200, 1600, 400), (0, 4, 1), 30);

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}

		void TestBracket((int lB, int uB, int inc) BoidRange, (int lB, int uB, int inc) ThreadRange, int ticks) {
			//(int, int, int) => lowerBound, upperBound (exclusive), increment

			var data = "";
			for (int i = BoidRange.lB; i < BoidRange.uB; i += BoidRange.inc)
				data += "\t" + i + " Boids";

			for (int j = ThreadRange.lB; j < ThreadRange.uB; j += ThreadRange.inc) {
				data += "\n" + j + " Threads\t";
				for (int i = BoidRange.lB; i < BoidRange.uB; i += BoidRange.inc) {
					data += Math.Ceiling(Test(i, j, ticks)) + "\t";
				}
			}

			//Write data to file
			StreamWriter SW = new StreamWriter("MultiBoidData.txt");
			SW.Write(data);
			SW.Dispose();
			Console.WriteLine("Saved data to path: " + Path.GetFullPath("MultiBoidData.txt"));

			double Test(int BoidCount, int ThreadCount, int ticks) {
				Console.WriteLine(String.Format(">{0} boids running on {1} threads.", BoidCount, ThreadCount));
				//Initilize
				boidCount = BoidCount;
				threadCount = ThreadCount;
				boidPos.Clear();
				boidVel.Clear();
				Random rand = new Random();
				for (int i = 0; i < BoidCount; i++) {
					boidPos.Add(new Vector(rand.Next(fieldSize), rand.Next(fieldSize)));
					boidVel.Add(new Vector(rand.Next(50), rand.Next(50)));
				}

				//Main Test
				var t0 = DateTime.Now;
				for (int i = 0; i < ticks; i++)
					Tick(0.03f);
				var time = (DateTime.Now - t0).TotalMilliseconds / ticks;
				Console.WriteLine(String.Format("This took {0} milliseconds.", Math.Ceiling(time)));
				return time;
			}
		}

		void Tick(float dT) {
			var outVel = new List<Vector>(new Vector[boidVel.Count]);
			int atIndex = 0;
			int threadSize = threadCount >= 0 ? boidCount / (threadCount + 1) : 0;
			List<Thread> workerThreads = new List<Thread>();

			//Start worker threads
			for (int i = 0; i < threadCount; i++) {
				int a = atIndex, b = (atIndex + threadSize);
				workerThreads.Add(new Thread(new ThreadStart(() => Phys(a, b))));
				workerThreads[i].Name = "Worker thread " + i;
				workerThreads[i].Start();
				atIndex += threadSize;
			}

			//Main thread works too
			Phys(atIndex, outVel.Count);

			//Await all threads
			foreach (var thread in workerThreads)
				thread.Join();

			boidVel = outVel;

			for (int i = 0; i < boidPos.Count; i++) {
				//Update position
				boidPos[i] += boidVel[i] * dT;
				boidPos[i] = new Vector(boidPos[i].x % (float)fieldSize, boidPos[i].y % (float)fieldSize);
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
	}
}