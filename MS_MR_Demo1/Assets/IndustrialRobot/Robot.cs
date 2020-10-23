using RobotDynamics.MathUtilities;
using RobotDynamics.Robots;
using RosSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public List<GameObject> Joints;
    public List<double> currentAngles;
    public GameObject Target;
    FanucCR7 CRobot = new FanucCR7();

    [Range(0, 1)]
    public float Lambda = 0.001f;
    [Range(0, 1)]
    public float Alpha = 0.05f;
    public int MaxIter = 100;
    [Range(0, 5)]
    [Tooltip("Used for smoothing the angles")]
    public float kp = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public static Vector ToVector(Vector3 v)
    {
        return new Vector(v.x, v.y, v.z);
    }

    Vector3 lastPos;
    Quaternion lastRot;

    double[] lastTargetAngles;
    int skipCounter = 0;
    public int skipCounterThreshold = 5;
    // Update is called once per frame
    void Update()
    {
        try
        {

            if (Target.transform.localPosition != lastPos || Target.transform.localRotation != lastRot)
            {
                if (skipCounter++ > skipCounterThreshold)
                {
                    skipCounter = 0;
                    //Inverse Kinematics
                    Vector r_des = ToVector(Target.transform.localPosition);
                    //RotationMatrix C_des = QuatToRotationMatrix(rotation);
                    var v = Target.transform.localEulerAngles;
                    RotationMatrix C_des = EulerAnglesToRotationMatrix(v.x, v.y, v.z);

                    lastPos = Target.transform.localPosition;
                    lastRot = Target.transform.localRotation;

                    var q = CRobot.ComputeInverseKindematics(r_des, C_des, Lambda, Alpha, MaxIter);

                    if (q.Any(x => double.IsNaN(x)))
                    {
                        Debug.Log("was NaN value");
                        return;
                    }

                    if (currentAngles == null || currentAngles.Count == 0)
                    {
                        currentAngles = q.ToList();
                    }

                    lastTargetAngles = q;
                }
            }
        }
        catch
        {

        }

        //Update robot
        SetQ(GetSmoothedAngles(lastTargetAngles));
    }

    private void SetQ(double[] q)
    {
        var transformations = CRobot.ComputerForwardKinematics(q);

        for (int i = 0; i < transformations.Count - 1; i++)
        {
            Vector p = transformations[i + 1].GetPosition();

            Joints[i].transform.localPosition = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
            RotationMatrix R = transformations[i + 1].GetRotation();

            try
            {
                Joints[i].transform.localRotation = FromRotationMatrx(R).rotation;
            }
            catch
            {

            }

            currentAngles[i] = (float)q[i];
        }
    }

    private double[] GetSmoothedAngles(double[] targetAngles)
    {
        double[] smoothed = new double[targetAngles.Length];
        for (int i = 0; i < targetAngles.Length; i++)
        {
            smoothed[i] = currentAngles[i] + (targetAngles[i] - currentAngles[i]) * kp * Time.deltaTime;
        }
        return smoothed;
    }

    private Matrix4x4 FromRotationMatrx(RotationMatrix R)
    {
        Vector4 c0 = new Vector4();
        Vector4 c1 = new Vector4();
        Vector4 c2 = new Vector4();
        Vector4 c3 = new Vector4();
        for (int i = 0; i < 3; i++)
        {
            c0[i] = (float)R.matrix[i, 0];
            c1[i] = (float)R.matrix[i, 1];
            c2[i] = (float)R.matrix[i, 2];
        }
        c3[3] = 1;
        var m = new Matrix4x4(c0, c1, c2, c3);
        return m;
    }

    private RotationMatrix QuatToRotationMatrix(Quaternion quat)
    {
        double qw = quat.w;
        RotationMatrix matrix = null;
        var qn = new Matrix(new double[,] { { quat.x }, { quat.y }, { quat.z } });
        double[,] m = new double[,]
        {
            { 0, -quat.z, quat.y },
            { quat.z, 0 , -quat.x },
            { -quat.y, quat.x,0 }
        };
        matrix = new RotationMatrix(m);
        matrix = new RotationMatrix(((2 * qw * qw - 1) * Matrix.Eye(3) + 2 * qw * matrix + 2 * (qn * qn.Transpose())).matrix);
        return matrix;
    }


    private RotationMatrix EulerAnglesToRotationMatrix(double x, double y, double z)
    {
        x = DegToRad(x);
        y = DegToRad(y);
        z = DegToRad(z);


        Matrix R1 = new Matrix(new double[,]
       {
            { Math.Cos(y), 0, Math.Sin(y) },
            { 0,1,0 },
            {-Math.Sin(y), 0, Math.Cos(y) } });

        Matrix R2 = new Matrix(new double[,]
        {
            { 1,0,0 },
            {0, Math.Cos(x), -Math.Sin(x) },
            {0, Math.Sin(x), Math.Cos(x) } });
        Matrix R3 = new Matrix(new double[,]
        {
            { Math.Cos(z), -Math.Sin(z), 0 },
            {Math.Sin(z), Math.Cos(z), 0 },
            {0,0,1 } });

        RotationMatrix m = new RotationMatrix((R1 * R2 * R3).matrix);
        return m;
    }

    private double DegToRad(double deg)
    {
        return deg / 360.0 * 2 * Math.PI;
    }
}
