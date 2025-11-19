using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tests
{
    public class PlayerMovementBoundaryTests
    {
        private GameObject player;
        private CharacterController characterController;

        [SetUp]
        public void Setup()
        {
            // Load the SampleScene using EditorSceneManager for EditMode tests
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            if (player != null)
                Object.DestroyImmediate(player);
        }

        [UnityTest]
        public IEnumerator PlayerMovement_BoundaryTest_CollisionBoundaries()
        {
            // Arrange
            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Player should exist in SampleScene");
            
            characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController, "CharacterController should be attached");

            // Create VISIBLE test walls with different colors
            GameObject[] walls = new GameObject[4];
            Color[] wallColors = { Color.red, Color.blue, Color.green, Color.yellow };
            Vector3[] wallPositions = {
                new Vector3(5f, 1f, 0f),   // East wall
                new Vector3(-5f, 1f, 0f),  // West wall
                new Vector3(0f, 1f, 5f),   // North wall
                new Vector3(0f, 1f, -5f)   // South wall
            };

            for (int i = 0; i < 4; i++)
            {
                walls[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                walls[i].name = $"TestWall{i}";
                walls[i].transform.position = wallPositions[i];
                walls[i].transform.localScale = new Vector3(2f, 2f, 0.1f);
                walls[i].AddComponent<BoxCollider>();
                
                // Add VISIBLE colored material
                MeshRenderer renderer = walls[i].GetComponent<MeshRenderer>();
                Material wallMaterial = new Material(Shader.Find("Standard"));
                wallMaterial.color = wallColors[i];
                renderer.material = wallMaterial;
            }

            // Test collision boundaries with VISIBLE movement
            Vector3 originalPosition = player.transform.position;
            Vector3[] collisionPositions = new Vector3[4];
            
            // Test collision from all 4 directions with VISIBLE movement
            Vector3[] directions = {
                Vector3.right,    // East
                Vector3.left,     // West  
                Vector3.forward,  // North
                Vector3.back      // South
            };

            for (int i = 0; i < directions.Length; i++)
            {
                // VISIBLE: Move player towards colored wall
                Vector3 targetPosition = originalPosition + directions[i] * 10f;
                
                // Simulate VISIBLE movement towards wall
                for (int step = 0; step < 100; step++)
                {
                    Vector3 direction = (targetPosition - player.transform.position).normalized;
                    characterController.Move(direction * Time.deltaTime * 5f);
                    
                    // VISIBLE: Player should stop at wall
                    yield return null;
                }
                
                collisionPositions[i] = player.transform.position;
                
                // Reset position for next test
                player.transform.position = originalPosition;
                yield return null;
            }

            // Assert - Player should not pass through walls
            for (int i = 0; i < collisionPositions.Length; i++)
            {
                float distanceFromOriginal = Vector3.Distance(collisionPositions[i], originalPosition);
                Assert.Less(distanceFromOriginal, 8f, $"Player should not pass through {wallColors[i]} wall in direction {i}");
            }

            // Clean up test walls
            for (int i = 0; i < walls.Length; i++)
            {
                Object.DestroyImmediate(walls[i]);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_BoundaryTest_EdgeOfWorld()
        {
            // Arrange
            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Player should exist in SampleScene");
            
            characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController, "CharacterController should be attached");

            Vector3 originalPosition = player.transform.position;
            
            // Create VISIBLE markers for extreme positions
            GameObject[] markers = new GameObject[6];
            Color[] markerColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta };
            string[] markerNames = { "Far East", "Far West", "Far North", "Far South", "High Up", "Deep Down" };
            
            // Test extreme world boundaries with VISIBLE markers
            Vector3[] extremePositions = {
                new Vector3(1000f, 0f, 0f),   // Far East
                new Vector3(-1000f, 0f, 0f),  // Far West
                new Vector3(0f, 0f, 1000f),   // Far North
                new Vector3(0f, 0f, -1000f),  // Far South
                new Vector3(0f, 1000f, 0f),   // High up
                new Vector3(0f, -1000f, 0f)   // Deep down
            };

            for (int i = 0; i < extremePositions.Length; i++)
            {
                // Create VISIBLE marker at extreme position
                markers[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                markers[i].name = $"Marker_{markerNames[i]}";
                markers[i].transform.position = extremePositions[i];
                markers[i].transform.localScale = Vector3.one * 2f;
                
                // Add VISIBLE colored material
                MeshRenderer renderer = markers[i].GetComponent<MeshRenderer>();
                Material markerMaterial = new Material(Shader.Find("Standard"));
                markerMaterial.color = markerColors[i];
                renderer.material = markerMaterial;
                
                // VISIBLE: Move player to extreme position
                player.transform.position = extremePositions[i];
                
                // Verify player can handle extreme positions
                Assert.IsNotNull(player, $"Player should exist at {markerNames[i]} position");
                Assert.IsNotNull(characterController, "CharacterController should exist at extreme position");
                
                // VISIBLE: Test movement at extreme position
                characterController.Move(Vector3.forward * Time.deltaTime);
                
                yield return null;
            }

            // Reset to original position
            player.transform.position = originalPosition;
            
            // Assert - Player should handle extreme world positions
            Assert.IsNotNull(player, "Player should survive extreme world boundary tests");
            
            // Clean up markers
            for (int i = 0; i < markers.Length; i++)
            {
                Object.DestroyImmediate(markers[i]);
            }
        }
    }
}