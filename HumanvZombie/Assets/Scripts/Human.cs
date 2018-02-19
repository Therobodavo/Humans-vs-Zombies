using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Human Class
 * Code to move Human
 * Programmed by David Knolls
 */
public class Human : Vehicle {

    //Prefabs used
    public GameObject humanTarget;
    public GameObject zombieChaser;

    //Weights used
    public float seekWeight;
    public float fleeWeight;

    //Is human fleeing from zombie?
    public bool fleeing;

	// Use this for initialization
	public override void Start () 
    {
        fleeing = false;
		base.Start();
	}

    //Calculates steering forces
    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = new Vector3(0, 0, 0);

        //if zombie is too close
        if(fleeing)
        {
            //Smart Flee
            ultimateForce += Evade(zombieChaser) * fleeWeight;
        }
        else
        {   
            //Wander around the world
            ultimateForce += Wander();
        }

        //Avoid obstacles
        ultimateForce += ObstacleAvoidance();

        //Seperate from other creatures
        ultimateForce += Seperate();

        //Stay in bounds
        ultimateForce += Bounds();

        ultimateForce.y = 0;

        ultimateForce = Vector3.ClampMagnitude(ultimateForce, maxForce);

        ApplyForce(ultimateForce);
    }

    //Displays debug lines/objects
    void OnRenderObject()
    {
        //If debug is on
        if(GameObject.Find("Manager").GetComponent<ExerciseManager>().debug) 
        {
            //Draws forward vector
            green.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(gameObject.transform.position);
            GL.Vertex(gameObject.transform.position + gameObject.transform.forward);
            GL.End();

            //Draws right vector
            blue.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(gameObject.transform.position);
            GL.Vertex(gameObject.transform.position + gameObject.transform.right);
            GL.End();

            //Draws future position
            gameObject.transform.FindChild("Human_FP").gameObject.SetActive(true);
            gameObject.transform.FindChild("Human_FP").transform.position = vehiclePosition + velocity;
        }
        //If debug is off
        else
        {
            gameObject.transform.FindChild("Human_FP").gameObject.SetActive(false);
        }
    }
}
