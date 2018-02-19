using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Manager Class
 * Code to get collision and manage objects
 * Programmed by David Knolls
 */
public class ExerciseManager : MonoBehaviour
{
    //Prefabs used
    public GameObject human;
    public GameObject zombie;
    public GameObject medPack;

    //Used for positioning
    public float screenWidth;

    //Lists to store humans, zombies, and med packs
    public List<GameObject> humans = new List<GameObject>();
    public List<GameObject> zombies = new List<GameObject>();
    public List<GameObject> medPacks = new List<GameObject>();

    //Is debug on or off
    public bool debug = false;

    //What should be spawned on mouse click
    public int manualSpawn = 0;

    //Temp object used for default values
    GameObject temp;

    //Default camera used
    public Camera cam;

    // Use this for initialization
    void Start ()
    {
        //Set Camera to main camera
        cam = Camera.FindObjectOfType<Camera>();

        //Set up default object
        temp = new GameObject();
        temp.transform.position = new Vector3(100,.25f,100);

        //Set screen width
        screenWidth = 9.5f;

        //Creates humans
		for(int i = 0; i < 10; i++)
        {
            humans.Add(createRandomHuman());
        }

        //Creates zombie
        zombie.transform.position = new Vector3(Random.Range(-screenWidth,screenWidth), .25f, Random.Range(-screenWidth,screenWidth));
        zombie.GetComponent<Zombie>().zombieTarget = humans[0];
        zombies.Add(Instantiate(zombie));

        //Updates the zombie's target/nearest human
        updateZombieTarget();
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Updates Zombie's target
		updateZombieTarget();

        //Checks if humans should run away
        humanCheckForZombies();

        //Checks for collisions on med packs
        zombieCheckMedPack();

        //Gets user input
        getInput();
	}

    //Gets all user input
    public void getInput()
    {
        getKeyInput();
        getMouseInput(); 
    }

    //Gets user input from keyboard
    public void getKeyInput()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            debug = !debug;
        }
        if(Input.GetKeyDown(KeyCode.F)) 
        {
            manualSpawn++;
            if(manualSpawn >= 3)
            {
                manualSpawn = 0;
            }
        }
    }

    //Gets user input from mouse
    public void getMouseInput() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 position = Vector3.zero;
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if(Physics.Raycast(ray.origin,ray.direction, out hit))
            {
                position = hit.point;
                position.y = 0.25f;

                if(manualSpawn == 0) 
                {
                    human.transform.position = position;
                    humans.Add(Instantiate(human));
                }
                else if(manualSpawn == 1)
                {
                    zombie.transform.position = position;
                    zombies.Add(Instantiate(zombie));
                    updateZombieTarget();
                }
                else
                {
                    medPack.transform.position = position;
                    medPacks.Add(Instantiate(medPack));
                }
            }
        }
    }

    //Creates a random human
    public GameObject createRandomHuman()
    {
        human.transform.position = new Vector3(Random.Range(-screenWidth,screenWidth), .25f, Random.Range(-screenWidth,screenWidth));
        return Instantiate(human);
    }

    //Updates the Zombie's target
    public void updateZombieTarget()
    {
        for(int i = 0; i < zombies.Count;i++)
        {
            GameObject closest = temp;
            for(int hum = 0; hum < humans.Count;hum++)
            {
                if((humans[hum].transform.position - zombies[i].transform.position).magnitude < (closest.transform.position - zombies[i].transform.position).magnitude)
                {
                    closest = humans[hum];
                }
            }
            if(closest != temp)
            {
                zombies[i].GetComponent<Zombie>().zombieTarget = closest;
            }
        }
    }

    //Checks if zombies collide with med packs, and if so turn zombies into humans
    public void zombieCheckMedPack()
    {
        bool reset = false;
        for(int i = 0; i < zombies.Count; i++) 
        {
            for(int meds = 0; meds < medPacks.Count; meds++) 
            {
                //If zombies collide with medpack
                if((zombies[i].transform.position - medPacks[meds].transform.position).magnitude < zombies[i].GetComponent<Vehicle>().radius + .4f)
                {
                    //Set up human bing created
                    human.transform.position = zombies[i].transform.position;
                    human.transform.rotation = zombies[i].transform.rotation;
                    human.GetComponent<Human>().fleeing = false;

                    //Destroy zombie
                    Destroy(zombies[i]);
                    zombies.RemoveAt(i);

                    //Create human
                    humans.Add(Instantiate(human));

                    //Destroy medpack
                    Destroy(medPacks[meds]);
                    medPacks.RemoveAt(meds);

                    meds--;
                    i--;
                    reset = true;

                    humanCheckForZombies();
                    break;
                }
            }
            if(reset)
            {
                break;
            }
        }
        reset = false;
    }

    //Humans check if they need to run away from a zombie
    public void humanCheckForZombies()
    {
        for(int i = 0; i < humans.Count; i++)
        {
            for(int zom = 0; zom < zombies.Count;zom++)
            {
                //Is there a zombie in range
                if((humans[i].transform.position - zombies[zom].transform.position).magnitude < 6f)
                {
                    humans[i].GetComponent<Human>().zombieChaser = zombies[zom];
                    humans[i].GetComponent<Human>().fleeing = true;
                    break;
                }
                else
                {
                    humans[i].GetComponent<Human>().fleeing = false;
                }

                if(humans[i].GetComponent<Human>().zombieChaser == null && !humans[i].GetComponent<Human>().fleeing)
                {
                    humans[i].GetComponent<Human>().fleeing = false;
                }
            }
        }
    }

    void OnGUI() 
    {
        string display = "Currently Spawning: ";
        if(manualSpawn == 0)
        {
            display += "Human";
        }
        else if(manualSpawn == 1)
        {
            display += "Zombie";
        }
        else
        {
            display += "Med Pack";
        }
        display += "\nPress F to switch\nPress D to toggle Debug lines.";
        GUI.Box(new Rect(10,10,200,55),display);
    }
}
