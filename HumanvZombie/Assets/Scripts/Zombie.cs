using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Zombie Class
 * Code to move Zombie
 * Programmed by David Knolls
 */
public class Zombie : Vehicle {

    //Human that the zombie is chasing
    public GameObject zombieTarget;

    //Generic Zombie prefab
    public GameObject zombiePrefab;

    //Weight used
    public float seekWeight;

    //Is the zombie wandering or chasing
    public bool wandering = false;

	// Use this for initialization
	public override void Start ()
    {
		base.Start();
	}

    public override void Update()
    {
        //If there are humans
        if(GameObject.Find("Manager").GetComponent<ExerciseManager>().humans.Count > 0)
        {
            checkCollision();
        }
        base.Update();
    }

    //Calculates steering forces
    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = new Vector3(0, 0, 0);
        try
        {
            //If there are humans and zombie is far enough away from human, smart seek
            if(GameObject.Find("Manager").GetComponent<ExerciseManager>().humans.Count > 0 && (vehiclePosition - zombieTarget.transform.position).magnitude > 1f)
            {
                ultimateForce += Pursue(zombieTarget)  * seekWeight;   
            }
            //If zombie is close to human, quick Seek
            else if(GameObject.Find("Manager").GetComponent<ExerciseManager>().humans.Count > 0 && (vehiclePosition - zombieTarget.transform.position).magnitude <= 1f) 
            {
                ultimateForce += Seek(zombieTarget.transform.position) * seekWeight;
            }
            else
            {
                //If there's no humans, wander around the world
                ultimateForce += Wander();
            }
        }
        catch(Exception e)
        {
            //If there's no humans, wander around the world
            ultimateForce += Wander();
        }

        //Avoid Obstacles
        ultimateForce += ObstacleAvoidance();

        //Seperate from close creatures
        ultimateForce += Seperate();

        //Stay in bounds
        ultimateForce += Bounds();

        ultimateForce.y = 0;

        ultimateForce = Vector3.ClampMagnitude(ultimateForce, maxForce);

        ApplyForce(ultimateForce);
    }

    //Checks collisions with humans
    public void checkCollision()
    {
        List<GameObject> tempHumans = GameObject.Find("Manager").GetComponent<ExerciseManager>().humans;
        List<GameObject> tempZombies = GameObject.Find("Manager").GetComponent<ExerciseManager>().zombies;
        for(int i = 0; i < GameObject.Find("Manager").GetComponent<ExerciseManager>().humans.Count;i++)
        {
            //If human and zombie collide
            if((tempHumans[i].transform.position - vehiclePosition).magnitude <= .75f)
            {
                //Set a zombie to be created where the human is
                zombiePrefab.transform.position = tempHumans[i].transform.position;
                zombiePrefab.transform.rotation = tempHumans[i].transform.rotation;

                //Destroy the human
                Destroy(tempHumans[i]);
                tempHumans.RemoveAt(i);
                i--;

                //Create the zombie
                zombiePrefab.GetComponent<Zombie>().zombieTarget = null;
                zombiePrefab.name = "Zombie_Horde";
                tempZombies.Add(Instantiate(zombiePrefab));

                 GameObject.Find("Manager").GetComponent<ExerciseManager>().updateZombieTarget();
                 GameObject.Find("Manager").GetComponent<ExerciseManager>().humanCheckForZombies();
            }
        }

        GameObject.Find("Manager").GetComponent<ExerciseManager>().humans = tempHumans;
        GameObject.Find("Manager").GetComponent<ExerciseManager>().zombies = tempZombies;
    }
    //Display debug stuff
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
        
            //Draws target
            if(GameObject.Find("Manager").GetComponent<ExerciseManager>().humans.Count > 0 && zombieTarget != null)
            {
                black.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(gameObject.transform.position + gameObject.transform.forward);
                GL.Vertex(zombieTarget.transform.position);
                GL.End();
            }

            //Draws future position
            gameObject.transform.FindChild("Zombie_FP").gameObject.SetActive(true);
            gameObject.transform.FindChild("Zombie_FP").position = vehiclePosition + velocity;
        }
        //If debug is off
        else
        {
            gameObject.transform.FindChild("Zombie_FP").gameObject.SetActive(false);
        }
    }
}
