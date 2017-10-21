using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour 
{
	public GameObject treeToSpawn;
	public int howManyTreeDoYouWant;

	List<Vector3> _treeSpawned;
	List<GameObject> _forest;
	SphereCollider _zone;
	int currentTry = 0;
	int maxTry = 1000;
	
	void Start () {
		_zone = GetComponent<SphereCollider>();
		_treeSpawned = new List<Vector3>();
		_forest = new List<GameObject>();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.C)) GenerateForest();
		if(Input.GetKeyDown(KeyCode.X))
		{
			foreach(GameObject o in _forest.ToArray())
			{
				Destroy(o);
			}
			_forest.Clear();
		}
	}
	
	void GenerateForest() 
	{
		for(int i = 0; i < howManyTreeDoYouWant; i++)
		{
			spawnTree();
		}
	}

	void spawnTree()
	{
		Vector3 rayOrigin = transform.position + new Vector3(Random.Range(-_zone.radius,_zone.radius), 0, Random.Range(-_zone.radius,_zone.radius));
		RaycastHit hit;
		currentTry++;

		if(Physics.Raycast(rayOrigin, Vector3.down, out hit, 100))
		{
			foreach(Vector3 tree in _treeSpawned)
			{
				if(Vector3.Distance(tree,hit.point) < 0.5f)
				{
					if(currentTry <= maxTry) spawnTree();
					else Debug.LogError("MaxTryError");
					return;
				}
			}
			_treeSpawned.Add(hit.point);
			_forest.Add(Instantiate(treeToSpawn, hit.point, Quaternion.identity));
		}
	}
}
