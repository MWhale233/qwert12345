using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    public List<GameObject> environmentPrefabs = new List<GameObject>();

    private List<GameObject> instances = new List<GameObject>();

    public List<Collider> restrictedBounds = new List<Collider>();

    public int numObjects = 5; // 30 objects are needed, use 5 to test

    public Vector3 generatorBoundsMin = new Vector3(-30, 0, -30);

    public Vector3 generatorBoundsMax = new Vector3(30, 0, 30);

    public bool reset = false;

    // Start is called before the first frame update
    void Start()
    {
        // Your code for Exercise 1.1 part 1.) here
        GenerateEnvironment();

    }

    // Update is called once per frame
    void Update()
    {
        // Your code for Exercise 1.1 part 3.) here
        if (reset)
        {
            ClearEnvironment();
            GenerateEnvironment();
            reset = false;
        }
    }

    void ClearEnvironment()
    {
        // Your code for Exercise 1.1 part 3.) here
        foreach (GameObject instance in instances)
        {
            Destroy(instance);
        }
        instances.Clear();
    }

    void GenerateEnvironment()
    {
        // Code for Exercise 1.1 part 1
        for (int i = 0; i < numObjects; i++)  // Loop to generate the specified number of objects
        {
            // Select a random prefab from the environmentPrefabs list
            int randomIndex = Random.Range(0, environmentPrefabs.Count);
            GameObject randomPrefab = environmentPrefabs[randomIndex];

            // Generate a random position within the specified bounds
            float randomX = Random.Range(generatorBoundsMin.x, generatorBoundsMax.x);
            float randomZ = Random.Range(generatorBoundsMin.z, generatorBoundsMax.z);
            Vector3 newPosition = new Vector3(randomX, 0, randomZ);
            
            // Instantiate the selected prefab and set the current GameObject as its parent
            GameObject newInstance = Instantiate(randomPrefab, newPosition, Quaternion.identity, gameObject.transform);

            // Apply a random rotation to the instance
            float randomYRotation = Random.Range(0f, 360f);
            newInstance.transform.rotation = Quaternion.Euler(0, randomYRotation, 0);

            // Store the instance in the instances list
            instances.Add(newInstance);
        }
        // Start the coroutine to resolve any collisions
        StartCoroutine(ResolveCollisions());
    }

    IEnumerator ResolveCollisions()
    {
        yield return new WaitForSeconds(2);
        bool resolveAgain;
        // Your code for Exercise 1.1 part 2.) here
        do
        {
            resolveAgain = false;

            foreach (GameObject instance in instances)
            {
                Collider[] colliders = Physics.OverlapSphere(instance.transform.position, 1f); // Use Physics to adjust the position as needed
                bool isOverlapping = false;

                // Check for overlaps with other instances
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject != instance && instances.Contains(collider.gameObject))
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                // Check for overlaps with restricted areas
                Collider instanceCollider = instance.GetComponent<Collider>();
                if (instanceCollider != null)
                {
                    foreach (Collider restrictedBound in restrictedBounds)
                    {
                        if (instanceCollider.bounds.Intersects(restrictedBound.bounds))
                        {
                            isOverlapping = true;
                            break;
                        }
                    }
                }

                // If an overlap is detected, move the instance to a new random position
                if (isOverlapping)
                {
                    float randomX = Random.Range(generatorBoundsMin.x, generatorBoundsMax.x);
                    float randomZ = Random.Range(generatorBoundsMin.z, generatorBoundsMax.z);
                    instance.transform.position = new Vector3(randomX, 0, randomZ);

                    // Apply a random rotation around the instance’s up-axis
                    float randomYRotation = Random.Range(0f, 360f);
                    instance.transform.rotation = Quaternion.Euler(0, randomYRotation, 0);

                    resolveAgain = true;
                }
            }

            // Wait a frame before checking for overlaps again to allow physics to update
            yield return null;

        } while (resolveAgain); // Keep resolving until there are no overlaps
    }

}

