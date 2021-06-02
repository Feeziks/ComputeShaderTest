# Testing Out Compute Shaders In Unity
## List of test scenes
1. RandomCubes
2. Boids (wip)

## RandomCubes
This scene shows how a GPU can massively speed up simple parallel tasks
For this contrived example we will change the material color of a GameObject
In this scene the CubeController script will control the number of cubes and the number of iterations that are performed
While running select either CPU or GPU randomization. Each will perform the randomization task and report the elapsed time in the editor console

Example results
![100 x 100 grid over 10000 iterations](https://github.com/Feeziks/ComputeShaderTest/blob/RandomCubes/Test_Data/RandomCubes/CPU_VS_GPU_100_x_100_x_10000.PNG)

##Boids
work in progress
