using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsSimulation
{
    public static Vector3 GetPosition(Vector3 Force, float mass, float time, Vector3 initialPosition, Vector3 initialSpeed)
    {
        Vector3 acceleration = GetAcceleration(Force, mass);
        Vector3 velocity = GetVelocity(acceleration, time, initialSpeed);

        return initialPosition + velocity * time;
    }

    public static Vector3 GetPosition(Vector3 velocity, float time, Vector3 initialPosition, Vector3 initialSpeed)
    {
        return initialPosition + velocity * time;
    }

    public static Quaternion GetRotation(Vector3 AngularForce, float mass, float time, float radius, Vector3 initialRotation, Vector3 initialSpeed)
    {
        Vector3 acceleration = GetAngularAcceleration(AngularForce, mass, radius);
        Vector3 velocity = GetVelocity(acceleration, time, initialSpeed);
        return Quaternion.Euler((initialRotation + velocity * time));
    }

    public static Vector3 GetAcceleration(Vector3 Force, float mass)
    {
        return Force / mass;
    }

    public static Vector3 GetAngularAcceleration(Vector3 AngularForce, float mass, float radius)
    {
        return AngularForce / (mass * radius * radius);
    }

    public static Vector3 GetVelocity(Vector3 Acceleration, float time, Vector3 initialSpeed)
    {
        Vector3 angularSpeed =  initialSpeed + Acceleration * time;
        angularSpeed = Vector3.ClampMagnitude(angularSpeed, 400);
        return angularSpeed;
    }

    public static Vector3 GetVelocity(Vector3 Force, float time, float mass, Vector3 initialSpeed)
    {
        Vector3 acceleration = Force / mass;
        return initialSpeed + acceleration * time;
    }

    
}
