using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;*/

namespace MultiBoid {
	public class MyProgram {
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
			//Populate with boids
			const int count = 800;
			Random rand = new Random();
			for (int i = 0; i < count; i++) {
				boidPos.Add(new Vector(rand.Next((int)window.Width), rand.Next((int)window.Height)));
				boidVel.Add(new Vector(rand.Next(50), rand.Next(50)));
			}
			Debug.WriteLine("WIDTH: " + window.Width + "HEIGHT: " + window.Height);
			stopwatch.Start();
			Update();
		}

		public void RecieveControl(object? sender, EventArgs args) {
			Update();
		}

		int frame = 0;
		int frameRatio = 4;

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

			float fixTickrate(int targetTickrate) {
				long targetDelay = 1000 / ((targetTickrate != 0) ? targetTickrate : 1);//Turn 1/s into ms
				long fixDelay = targetDelay - stopwatch.ElapsedMilliseconds;

				if (targetTickrate> 0 && fixDelay > 0)
					Thread.Sleep((int)fixDelay);

				float dT = stopwatch.ElapsedMilliseconds / 1000f;
				stopwatch.Restart();
				
				return dT;
			}
		}

		void Tick(float dT) {
			Debug.WriteLine(String.Format("Tickrate: {0}; Delay: {1}", 1/dT, dT*1000));
			for (int i = 0; i < boidPos.Count; i++) {
				//Calculate boid forces


				/*
				Coherence - steer towards others
				Seperation - steer away from others when too close
				Alignment - steer torwards velocity of others
				*/

				//C - 100% is looking right at the target

				float C = 0.001f;
				float S = 1f;
				float A = 0.01f;
				float B = 5f; //Barrier force

				for (int j = 0; j < boidPos.Count; j++) {
					if (i == j)
						continue;

					Vector dir = (boidPos[j] - boidPos[i]);

					boidVel[i] = boidVel[i].Rotate((dir.Angle - boidVel[i].Angle) * C * dT);// / dir.Length);
					//boidVel[i] += dir * C * dT;

					if (dir.Length <= 10)
						boidVel[i] = boidVel[i].Rotate((-dir.Angle - boidVel[i].Angle) * S * dT / dir.Length);
					/*float repDist = 10;
					if (dir.Length <= repDist)
						boidVel[i] -= dir.Normalized * (float)Math.Pow((repDist - dir.Length),2) * S * dT;*/

					boidVel[i] = boidVel[i].Rotate((boidVel[j].Angle - boidVel[i].Angle) * A * dT / dir.Length);
					//boidVel[i] += ((boidVel[i] * (1 - A) + boidVel[j] * A) - boidVel[i]) * dT;
				}

				//Update position
				boidPos[i] += boidVel[i] * dT;

				boidPos[i] = new Vector(boidPos[i].x % (float)window.Width, boidPos[i].y % (float)window.Height);
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
	}
}

/* To Do:

Proper boid simulation
	Revamp C/S/A calcs

Rendering Optimizations
	Re-use canvas for rendering

Memory Leak?

Implement Multithreading

Simulation Optimizations
	Quad tree / limit visual range

*/