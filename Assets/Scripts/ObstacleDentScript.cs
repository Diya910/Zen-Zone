// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ObstacleDentScript : MonoBehaviour
// {
//     Mesh deformingMesh;
//     Vector3[] originalVertices, displacedVertices;
//     Vector3[] vertexVelocities;
//     public float force = 10f;
//     public float forceOffset = 0.1f;

//     public void AddDeformingForce(Vector3 point, float force)
//     {
//         for (int i = 0; i < displacedVertices.Length; i++)
//         {
//             AddForceToVertex(i, point, force);
//         }
//     }

//     void AddForceToVertex(int i, Vector3 point, float force)
//     {
//         Vector3 pointToVertex = displacedVertices[i] - point;
//         float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
//         float velocity = attenuatedForce * Time.deltaTime;
//         vertexVelocities[i] += pointToVertex.normalized * velocity;
//     }

//     void UpdateVertex(int i)
//     {
//         Vector3 velocity = vertexVelocities[i];
//         displacedVertices[i] += velocity * Time.deltaTime;
//     }

//     void Start()
//     {
//         deformingMesh = GetComponent<MeshFilter>().mesh;
//         originalVertices = deformingMesh.vertices;
//         displacedVertices = new Vector3[originalVertices.Length];
//         for (int i = 0; i < originalVertices.Length; i++)
//         {
//             displacedVertices[i] = originalVertices[i];
//         }
//         vertexVelocities = new Vector3[originalVertices.Length];
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         Debug.Log("Hey");
//         Debug.Log(collision.gameObject.name);

//         if (collision.gameObject.name == "FirstPersonController")
//         {
//             Debug.Log("Entered");
//             for (int i = 0; i < displacedVertices.Length; i++)
//             {
//                 UpdateVertex(i);
//             }
//             deformingMesh.vertices = displacedVertices;
//             deformingMesh.RecalculateNormals();
//         }
//     }

//     void Update() { }
// }


using UnityEngine;

public class ObstacleDentScript : MonoBehaviour
{
    public float dentStrength = 0.3f;
    public float dentDecay = 0.05f;
    public int health = 40;

    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    public MeshDestroy md;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalVertices = meshFilter.mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
        originalVertices.CopyTo(modifiedVertices, 0);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "FirstPersonController")
        {
            Debug.Log("hey");
            foreach (ContactPoint contact in collision.contacts)
            {
                ModifyMesh(contact.point);
            }
            health -= 10;
            if (health % 4 == 0)
            {
                md.DestroyMesh();
            }
        }
    }

    void ModifyMesh(Vector3 point)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            float distance = Vector3.Distance(point, originalVertices[i]);
            float dentAmount = dentStrength / (distance + 1);
            modifiedVertices[i] -=
                transform.InverseTransformDirection(point - originalVertices[i]).normalized
                * dentAmount;
        }
        meshFilter.mesh.vertices = modifiedVertices;
    }

    void Update()
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] +=
                (originalVertices[i] - modifiedVertices[i]) * dentDecay * Time.deltaTime;
        }
        meshFilter.mesh.vertices = modifiedVertices;
    }
}
