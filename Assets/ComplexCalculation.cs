using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;

public struct VecMatPair
{
    public Vector3 pos;
    public Matrix4x4 mat;
}

public struct CalcJob : IJobParallelFor
{
    public NativeArray<Vector3> jobOutput;
    [ReadOnly] public NativeArray<Vector3> jobData;


    public void Execute(int i)
    {
        jobOutput[i] = jobData[i] + new Vector3(jobData[i].x + 1, jobData[i].y - 1, jobData[i].z * 2);
    }
}

public class ComplexCalculation : MonoBehaviour {

    public ComputeShader shader;
    public VecMatPair[] data1 = new VecMatPair[5000000];
    public VecMatPair[] output1 = new VecMatPair[5000000];

    public Vector3[] data = new Vector3[640000];
    public Vector3[] output = new Vector3[640000];
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            FillData();
        else if (Input.GetKeyDown(KeyCode.C))
            Calculate();
        else if (Input.GetKeyDown(KeyCode.G))
            RunShader();
        else if (Input.GetKeyDown(KeyCode.J))
            JobCalculation();
    }

    void FillData()
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        for (int i = 1; i < data.Length; i++)
        {
            data[i] = new Vector3(i, i * i, i / (i + 1));
        }
        stopWatch.Stop();
        UnityEngine.Debug.Log("<color=white>" + stopWatch.Elapsed.ToString() + "</color>");
    }

    void Calculate()
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 0; i < data.Length; i++)
        {
            output[i] = data[i] + new Vector3(data[i].x+1, data[i].y-1, data[i].z*2);
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("<color=red>" + stopWatch.Elapsed.ToString() + "</color>");
    }

    void RunShader()
    {
        Stopwatch stopWatch = new Stopwatch();
        
        //VecMatPair[] data = new VecMatPair[5000];
        //VecMatPair[] output = new VecMatPair[5000];

        //INITIALIZE DATA HERE

        ComputeBuffer buffer = new ComputeBuffer(data.Length, 12);
        buffer.SetData(data);
        int kernel = shader.FindKernel("Multiply");
        shader.SetBuffer(kernel, "dataBuffer", buffer);

        stopWatch.Start();
        shader.Dispatch(kernel, 16, 16, 1);
        buffer.GetData(output);

        stopWatch.Stop();
        UnityEngine.Debug.Log("<color=green>" + stopWatch.Elapsed.ToString() + "</color>");
    }

   

    

    void JobCalculation()
    {
        List<JobHandle> jobHandles = new List<JobHandle>();
        var jobOutputCalculation = new NativeArray<Vector3>(640000, Allocator.Persistent);

        var jobDataCalculation = new NativeArray<Vector3>(640000, Allocator.Persistent);
        
        for (var i = 0; i < jobDataCalculation.Length; i++)
        {
            jobDataCalculation[i] = new Vector3(i, i * i , i / (i+1));
            
        }
        for (int i = 0; i < System.Environment.ProcessorCount; i++)
        {
            var job = new CalcJob()
            {
                jobData = jobDataCalculation,
                jobOutput = jobOutputCalculation
            };
            if (i == 0)
                jobHandles.Add(job.Schedule(jobDataCalculation.Length, 1000));
            else
                jobHandles.Add(job.Schedule(jobDataCalculation.Length, 1000, jobHandles[i-1]));
        }
        

        
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        jobHandles[jobHandles.Count-1].Complete();
        // Initialize the job data


        // Schedule a parallel-for job. First parameter is how many for-each iterations to perform.
        // The second parameter is the batch size,
        // essentially the no-overhead innerloop that just invokes Execute(i) in a loop.
        // When there is a lot of work in each iteration then a value of 1 can be sensible.
        // When there is very little work values of 32 or 64 can make sense.


        // Ensure the job has completed.
        // It is not recommended to Complete a job immediately,
        // since that reduces the chance of having other jobs run in parallel with this one.
        // You optimally want to schedule a job early in a frame and then wait for it later in the frame.

        stopWatch.Stop();

        UnityEngine.Debug.Log("<color=yellow>" + stopWatch.Elapsed.ToString() + "</color>");

        // Native arrays must be disposed manually.
        jobDataCalculation.Dispose();
        jobOutputCalculation.Dispose();
    }
}
