using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tests
{
    public class PlayerMovementStressTests
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
        public IEnumerator PlayerMovement_StressTest_ComponentStability()
        {
            // Arrange
            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Player should exist in SampleScene");
            
            characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController, "CharacterController should be attached");

            int iterations = 500;
            Vector3[] positions = new Vector3[iterations];
            bool[] componentStates = new bool[iterations];
            
            // Act - Component stability under stress
            for (int i = 0; i < iterations; i++)
            {
                // Apply extreme movement parameters
                characterController.height = Random.Range(0.5f, 10f);
                characterController.radius = Random.Range(0.1f, 5f);
                characterController.center = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );
                
                // Apply movement
                Vector3 movement = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                );
                characterController.Move(movement * Time.deltaTime * 5f);
                
                positions[i] = player.transform.position;
                componentStates[i] = characterController != null && player != null;
                
                yield return null;
            }

            // Assert - Components should remain stable
            for (int i = 0; i < iterations; i++)
            {
                Assert.IsTrue(componentStates[i], "Components should remain stable under stress");
            }
            
            // Movement validation
            Vector3 totalMovement = positions[iterations - 1] - positions[0];
            Assert.IsTrue(totalMovement.magnitude > 0.1f, "Player should have moved during stability test");
        }

        [UnityTest]
        public IEnumerator PlayerMovement_StressTest_ExtremeInputValues()
        {
            // Arrange
            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Player should exist in SampleScene");
            
            characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController, "CharacterController should be attached");

            int iterations = 200;
            Vector3[] positions = new Vector3[iterations];

            // Act - Test with extreme input values
            for (int i = 0; i < iterations; i++)
            {
                // Test extreme movement parameters
                float originalHeight = characterController.height;
                float originalRadius = characterController.radius;
                
                // Apply extreme values temporarily
                characterController.height = Random.Range(0.1f, 100f);
                characterController.radius = Random.Range(0.1f, 50f);
                
                // Apply some movement
                Vector3 movement = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                );
                characterController.Move(movement * Time.deltaTime);
                
                // Restore original values
                characterController.height = originalHeight;
                characterController.radius = originalRadius;
                
                positions[i] = player.transform.position;
                
                yield return null;
            }

            // Assert - System should handle extreme values gracefully
            Assert.IsNotNull(player, "Player should still exist");
            Assert.IsNotNull(characterController, "CharacterController should still exist");
        }

        [UnityTest]
        public IEnumerator PlayerMovement_StressTest_ConcurrentInputs()
        {
            // Arrange
            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Player should exist in SampleScene");
            
            characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController, "CharacterController should be attached");

            int iterations = 1000;
            Vector3[] positions = new Vector3[iterations];
            float[] frameTimes = new float[iterations];
            Vector3[] rotations = new Vector3[iterations];
            
            // Act - Concurrent mouse and keyboard input simulation
            for (int i = 0; i < iterations; i++)
            {
                float startTime = Time.realtimeSinceStartup;
                
                // Simulate concurrent keyboard input (WASD movement)
                Vector2 keyboardInput = new Vector2(
                    Random.Range(-1f, 1f), // A/D keys (left/right)
                    Random.Range(-1f, 1f)  // W/S keys (forward/backward)
                );
                
                // Simulate concurrent mouse input (look rotation)
                Vector2 mouseInput = new Vector2(
                    Random.Range(-180f, 180f), // Mouse X (horizontal look)
                    Random.Range(-90f, 90f)    // Mouse Y (vertical look)
                );
                
                // Apply keyboard movement
                Vector3 movement = new Vector3(keyboardInput.x, 0, keyboardInput.y);
                characterController.Move(movement * Time.deltaTime * 5f);
                
                // Apply mouse rotation
                Vector3 currentRotation = player.transform.eulerAngles;
                Vector3 newRotation = new Vector3(
                    currentRotation.x + mouseInput.y * Time.deltaTime,
                    currentRotation.y + mouseInput.x * Time.deltaTime,
                    currentRotation.z
                );
                player.transform.rotation = Quaternion.Euler(newRotation);
                
                // Record data
                positions[i] = player.transform.position;
                rotations[i] = player.transform.eulerAngles;
                
                float endTime = Time.realtimeSinceStartup;
                frameTimes[i] = endTime - startTime;
                
                yield return null;
            }

            // Assert - System should handle concurrent inputs without issues
            Assert.IsNotNull(player, "Player should still exist after concurrent input stress");
            Assert.IsNotNull(characterController, "CharacterController should still exist");
            
            // Performance assertions
            float averageFrameTime = 0f;
            for (int i = 0; i < frameTimes.Length; i++)
            {
                averageFrameTime += frameTimes[i];
            }
            averageFrameTime /= frameTimes.Length;
            
            Assert.Less(averageFrameTime, 0.1f, "Average frame time should be reasonable for concurrent inputs");
            
            // Movement validation - player should have moved from original position
            Vector3 totalMovement = positions[iterations - 1] - positions[0];
            Assert.IsTrue(totalMovement.magnitude > 0.1f, "Player should have moved during concurrent input test");
        }
    }
}