using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Vehicle Class
 * Moves the attached object and can flee/seek
 * Programmed by David Knolls
 */
public abstract class Vehicle : MonoBehaviour 
{

	// Vectors for force-based movement
	public Vector3 acceleration;
	public Vector3 direction;
	public Vector3 velocity;
	public Vector3 vehiclePosition;

	// Floats for force-based movement
	public float mass;
    public float maxSpeed;
    public float maxForce;
    public float radius;

    public float angle = 0;

    //Colors
    public Material red;
    public Material blue;
    public Material black;
    public Material green;
    public Material yellow;
    public Material purple;

    //Obstacles
    public GameObject[] obstacles;

    //Closest Obstacle
    public GameObject closest;

	// Use this for initialization
    public virtual void Start () 
	{
        obstacles=GameObject.FindGameObjectsWithTag("Tree");
        closest = obstacles[0];
	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
        //Calculate Steering Forces
        CalcSteeringForces();

        //Update the position of the vehicle
        UpdatePosition();

        //Set the direction the vehicle is facing
        SetTransform();
	}

	// void ApplyForce()
	// Applies a force to this vehicle's acceleration
	// Mass affects acceleration
	public void ApplyForce(Vector3 force)
	{
		acceleration += force / mass;
	}


    //Updates Position of vehicle
    public void UpdatePosition()
    {
        vehiclePosition = transform.position;

		// 1. Add accel to velocity
		velocity += acceleration * Time.deltaTime;

        //Clamps velocity
        velocity = Vector3.ClampMagnitude(velocity,maxSpeed);

		// 2. Add vel to pos
		vehiclePosition += velocity * Time.deltaTime;

		// 3. Get a normalized velocity as direction
		direction = velocity.normalized;

		// 4. Refresh accel
		acceleration = Vector3.zero;

        vehiclePosition.y = 0.25f;

        transform.position = vehiclePosition;
    }

    //Sets transform of vehicle (forward)
    public void SetTransform()
    {

        transform.forward = direction;
    }

    //Seek towards a target
    public Vector3 Seek(Vector3 target)
    {
        //Get desired velocity
        Vector3 desVel = target - vehiclePosition;

        //Normalize
        desVel.Normalize();

        //Set to max speed
        desVel *= maxSpeed;

        //Calculate steering force
        Vector3 steeringForce = desVel - velocity;
        return steeringForce;

    }

    //Flee from a target
    public Vector3 Flee(Vector3 target)
    {
        //Get desired velocity
        Vector3 desVel = (vehiclePosition - target);

        //Normalize
        desVel.Normalize();

        //Set to max speed
        desVel *= maxSpeed;

        //Calculate steering force
        Vector3 steeringForce = desVel - velocity;
        return steeringForce;
    }

    //Avoids Obstacles
    public Vector3 ObstacleAvoidance() 
    {
        Vector3 steeringForce = Vector3.zero;
        Vector3 desVel = Vector3.zero;
        Vector3 closestDis = Vector3.zero;

        //Finds closest Obstacle
        foreach(GameObject i in obstacles) 
        {
            closestDis = vehiclePosition - closest.transform.position;
            if((vehiclePosition - i.transform.position).magnitude < closestDis.magnitude)
            {
                closest = i;
                closestDis = vehiclePosition - closest.transform.position;
            }
        }
        
        //Updates distance
        closestDis = vehiclePosition - closest.transform.position;

        //Double checks the closest obstacle is actually the closest and matters
        if(closest == obstacles[0] && Vector3.Dot(closestDis,gameObject.transform.right) <= 0 || closestDis.magnitude > radius + closest.GetComponent<Obstacle>().radius + 1f)
        {
            return Vector3.zero;
        }

        //If in range of Vehicle
        if(radius + closest.GetComponent<Obstacle>().radius + .8f> closestDis.magnitude)
        {
            //Find if on the right or left
            if(Vector3.Dot(gameObject.transform.right, vehiclePosition - closest.transform.position) < 0)
            {
                //right hand
                desVel = -gameObject.transform.right;
            }
            else 
            {
                //left hand
                desVel = gameObject.transform.right;
            }
            //Sets desired velocity
            desVel.Normalize();
            desVel *= maxSpeed;

            //Sets steering force
            steeringForce = desVel - velocity;
        }
        return steeringForce * maxForce;
    }

    //Wander, returns a force to wander around the world
    public Vector3 Wander()
    {
        //Gets position of the circle
        Vector3 centerCircle = velocity.normalized;
        centerCircle *= 3f;

        //Gets displacement vector
        Vector3 displacementCircle = new Vector3(0,0,1);
        displacementCircle *= 2f;

        //Rotates displacement vector
        displacementCircle.x = Mathf.Cos(angle) * displacementCircle.magnitude;
        displacementCircle.z = Mathf.Sin(angle) * displacementCircle.magnitude;

        //Gets random angle
        angle += UnityEngine.Random.Range(-2,2);

        //Adds wandering force together
        Vector3 wanderingForce = centerCircle += displacementCircle;
        wanderingForce.y = 0;

        return wanderingForce;
    }

    //Pursue, smart seek
    public Vector3 Pursue(GameObject target) 
    {
        //Catches if an object is removed in high amounts but still runs this?
        try
        {
            float distance = (target.transform.position - vehiclePosition).magnitude;
            float multiplier = distance / maxSpeed;
            return Seek(target.transform.position + target.GetComponent<Vehicle>().velocity * multiplier);
        }
        catch(Exception ex)
        {
             return Vector3.zero;
        }
    }

    //Evade, smart flee
    public Vector3 Evade(GameObject target)
    {
        //Catches if evading from a deleted object?
        try
        {
            float distance = (target.transform.position - vehiclePosition).magnitude;
            float multiplier = distance / maxSpeed;
            return Flee(target.transform.position + target.GetComponent<Vehicle>().velocity * multiplier);
        }
        catch(Exception ex)
        {
            return Vector3.zero;
        }
    }

    //Seperate, moves objects that are close together away from each other
    public Vector3 Seperate()
    {
        Vector3 seperationForce = Vector3.zero;
        int count = 0;
        GameObject[] creatures = GameObject.FindGameObjectsWithTag("Creature"); 

        //Goes through each human and zombie
        for(int i = 0; i < creatures.Length;i++)
        {
            //If close together
            if((creatures[i].transform.position - vehiclePosition).magnitude <= .4f)
            {
                //Get seperation force
                seperationForce += Flee(creatures[i].transform.position);
                count++;
            }
        }

        //If there was at least one creature close by
        if(count > 0)
        {
            seperationForce /= count;
        }
        return seperationForce;
    }

    //Bounds, stay in bounds of the world
    public Vector3 Bounds()
    {
        Vector3 targetForce = Vector3.zero;

        //If out of bounds, seek the center
        if(vehiclePosition.x > 10f || vehiclePosition.x < -10f || vehiclePosition.z > 10f || vehiclePosition.z < -10f)
        {
            targetForce = Seek(Vector3.zero);
        }
        return targetForce;
    }

    //Calculates all steering forces
    public abstract void CalcSteeringForces();
}
