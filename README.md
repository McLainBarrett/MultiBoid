# MultiBoid
A custom C# implementation of the popular boids algorithm, which simulates flock behavior, such as with a flock of birds or
school of fish. This implementation adds multithreading optimization to boost performance and employ otherwise idle resources. The purpose of this project is to push the limits of a simple boid simulation with multithreading.

To install MultiBoid, simply download and extract the latest release.

To use, launch 'MazeMaker.exe'. This will run a benchmark of boid simulations. The program will show the number of boids being simulated, and the average tick duration for each thread count. This might take a while, expecially on the higher boid count rounds, so please be patient. 

Typically, higher thread counds reduce the tick duration, showing that the simulation can speed up with the use of multithreading.