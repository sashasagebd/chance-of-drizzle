using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests.PlayMode
{
    /// <summary>
    /// Visual Stress Tests - Run these in the Unity Editor to see visual feedback
    /// These tests are designed to stress test systems with extreme values while being observable
    /// Tests run in the PlayerTests scene for proper setup
    /// </summary>
    public class VisualStressTests
    {
        private GameObject playerObject;
        private GameObject weaponObject;
        private GameObject bulletPrefab;
        private PlayerController3D playerController;
        private CharacterController characterController;
        private ProjectileWeapon projectileWeapon;
        private WeaponInventory weaponInventory;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Load the PlayerTests scene
            yield return SceneManager.LoadSceneAsync("PlayerTests", LoadSceneMode.Single);
            yield return null;

            // Find the player in the scene
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                // If no player tagged, find by name or component
                playerObject = Object.FindObjectOfType<PlayerController3D>()?.gameObject;
            }

            Assert.IsNotNull(playerObject, "PlayerTests scene must contain a player object");

            playerController = playerObject.GetComponent<PlayerController3D>();
            characterController = playerObject.GetComponent<CharacterController>();
            weaponInventory = playerObject.GetComponent<WeaponInventory>();

            Assert.IsNotNull(playerController, "Player must have PlayerController3D component");
            Assert.IsNotNull(characterController, "Player must have CharacterController component");

            // Create bullet prefab for projectile weapon
            bulletPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulletPrefab.name = "TestBullet";
            bulletPrefab.transform.localScale = Vector3.one * 0.2f;
            bulletPrefab.transform.position = new Vector3(0, -1000, 0); // Move far away

            // Add Rigidbody to bullet
            var bulletRb = bulletPrefab.AddComponent<Rigidbody>();
            bulletRb.useGravity = false;
            bulletRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add Bullet script for proper behavior
            var bulletScript = bulletPrefab.AddComponent<Bullet>();
            bulletScript.damage = 10;
            bulletScript.lifetime = 5f;

            // Wait for Awake to run on the bullet
            yield return null;

            // Now deactivate after components are initialized
            bulletPrefab.SetActive(false);

            // Try to get existing weapon, or create a new test weapon
            if (weaponInventory != null && weaponInventory.Current != null)
            {
                weaponObject = weaponInventory.Current.gameObject;
                projectileWeapon = weaponObject.GetComponent<ProjectileWeapon>();
            }

            // If no ProjectileWeapon found, create one
            if (projectileWeapon == null)
            {
                weaponObject = new GameObject("StressTestWeapon");
                weaponObject.transform.SetParent(playerObject.transform);
                weaponObject.transform.localPosition = new Vector3(0.5f, 1.5f, 0.5f);

                projectileWeapon = weaponObject.AddComponent<ProjectileWeapon>();
                projectileWeapon.magazineSize = 999; // Unlimited ammo for stress test
                projectileWeapon.damage = 10;
                projectileWeapon.bulletPrefab = bulletPrefab;
                projectileWeapon.muzzleSpeed = 60f;
                projectileWeapon.useGravity = false;

                // Create muzzle transform
                GameObject muzzleObj = new GameObject("Muzzle");
                muzzleObj.transform.SetParent(weaponObject.transform);
                muzzleObj.transform.localPosition = Vector3.forward * 0.5f;
                projectileWeapon.muzzle = muzzleObj.transform;

                // Reload to sync ammo with magazineSize (Awake runs before we set magazineSize)
                projectileWeapon.Reload();
            }
            else
            {
                // Use existing weapon but configure for stress testing
                projectileWeapon.bulletPrefab = bulletPrefab;
                projectileWeapon.magazineSize = 999;
                projectileWeapon.Reload(); // Sync ammo with new magazineSize
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Clean up bullet prefab
            if (bulletPrefab != null) Object.Destroy(bulletPrefab);

            // Clean up any spawned bullets
            var bullets = Object.FindObjectsOfType<Bullet>();
            foreach (var bullet in bullets)
            {
                Object.Destroy(bullet.gameObject);
            }

            yield return null;
        }

        #region Input Stress Tests

        /// <summary>
        /// STRESS TEST: Rapid Movement Input Changes
        /// Watch in Editor: Player should handle rapid directional changes without breaking
        /// Duration: 5 seconds of extreme input changes
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_RapidInputChanges()
        {
            Debug.Log("=== STARTING INPUT STRESS TEST ===");
            Debug.Log("Watch the player object - it will rapidly change movement direction");

            // Test parameters
            float testDuration = 5f;
            float inputChangeInterval = 0.05f; // Change input 20 times per second
            float elapsed = 0f;
            int inputChanges = 0;

            Vector3 startPosition = playerObject.transform.position;

            while (elapsed < testDuration)
            {
                // Simulate random directional input
                float moveX = Random.Range(-1f, 1f);
                float moveZ = Random.Range(-1f, 1f);
                Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

                // Apply movement by directly manipulating the character controller
                // This simulates what the input system would do
                if (characterController != null && moveDir.magnitude > 0.1f)
                {
                    Vector3 motion = moveDir * playerController.runSpeed * Time.deltaTime;
                    characterController.Move(motion);

                    // Rotate player to face movement direction
                    playerObject.transform.rotation = Quaternion.LookRotation(moveDir);
                }

                inputChanges++;

                // Log progress every second
                if (inputChanges % 20 == 0)
                {
                    float distanceMoved = Vector3.Distance(startPosition, playerObject.transform.position);
                    Debug.Log($"[{elapsed:F1}s] Input changes: {inputChanges}, Distance moved: {distanceMoved:F2}m");
                }

                yield return new WaitForSeconds(inputChangeInterval);
                elapsed += inputChangeInterval;
            }

            // Final report
            float totalDistance = Vector3.Distance(startPosition, playerObject.transform.position);
            Debug.Log($"=== INPUT STRESS TEST COMPLETE ===");
            Debug.Log($"Total input changes: {inputChanges}");
            Debug.Log($"Total distance moved: {totalDistance:F2}m");
            Debug.Log($"Average changes per second: {inputChanges / testDuration:F1}");

            // Verify player didn't break
            Assert.IsNotNull(playerObject, "Player should still exist after stress test");
            Assert.IsNotNull(characterController, "CharacterController should still be functional");
            Assert.Greater(totalDistance, 0f, "Player should have moved during stress test");
        }

        /// <summary>
        /// STRESS TEST: Concurrent Actions (Move + Jump + Crouch)
        /// Watch in Editor: Player performs all actions simultaneously at high frequency
        /// Duration: 3 seconds
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_ConcurrentActions()
        {
            Debug.Log("=== STARTING CONCURRENT ACTIONS STRESS TEST ===");
            Debug.Log("Watch: Player will move, jump, and change states rapidly");

            float testDuration = 3f;
            float elapsed = 0f;
            int actions = 0;

            while (elapsed < testDuration)
            {
                // Random movement
                float moveX = Random.Range(-1f, 1f);
                float moveZ = Random.Range(-1f, 1f);
                Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized * playerController.runSpeed;

                if (moveDir.magnitude > 0.1f)
                {
                    characterController.Move(moveDir * Time.deltaTime);
                    playerObject.transform.rotation = Quaternion.LookRotation(moveDir);
                }

                // Random jump attempts (simulate rapid jump button presses)
                if (Random.value > 0.7f && characterController.isGrounded)
                {
                    // Simulate jump by applying upward velocity
                    characterController.Move(Vector3.up * playerController.jumpSpeed * Time.deltaTime);
                }

                // Random speed changes (simulate sprint toggle)
                if (Random.value > 0.8f)
                {
                    float speed = Random.value > 0.5f ? playerController.sprintSpeed : playerController.runSpeed;
                    moveDir = moveDir.normalized * speed;
                }

                actions++;

                if (actions % 50 == 0)
                {
                    Debug.Log($"[{elapsed:F1}s] Actions performed: {actions}");
                }

                yield return new WaitForSeconds(0.02f); // 50 actions per second
                elapsed += 0.02f;
            }

            Debug.Log($"=== CONCURRENT ACTIONS STRESS TEST COMPLETE ===");
            Debug.Log($"Total actions: {actions}");
            Debug.Log($"Actions per second: {actions / testDuration:F1}");

            Assert.IsNotNull(playerObject, "Player should survive concurrent action stress");
            Assert.IsNotNull(characterController, "CharacterController should remain functional");
        }

        #endregion

        #region Fire Rate Stress Tests

        /// <summary>
        /// STRESS TEST: Extreme Fire Rate
        /// Watch in Editor: Bullets will spawn at extremely high rate (visible projectile spam)
        /// Duration: 3 seconds at 100 rounds per second
        /// WARNING: This will spawn 300+ bullets - very visible!
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_ExtremeFireRate()
        {
            Debug.Log("=== STARTING EXTREME FIRE RATE STRESS TEST ===");
            Debug.Log("Watch the scene view - bullets will spawn rapidly!");
            Debug.Log("WARNING: 100 bullets per second for 3 seconds = 300 bullets!");

            // Set extreme fire rate
            float targetFireRate = 100f; // 100 shots per second
            projectileWeapon.fireRate = targetFireRate;

            float testDuration = 3f;
            float elapsed = 0f;
            int bulletsFired = 0;
            int fireAttempts = 0;

            Vector3 fireOrigin = projectileWeapon.muzzle.position;
            Vector3 fireDirection = Vector3.forward;

            // Continuously fire for test duration
            while (elapsed < testDuration)
            {
                bool fired = projectileWeapon.TryFire(fireOrigin, fireDirection);
                fireAttempts++;

                if (fired)
                {
                    bulletsFired++;

                    // Log every 50 bullets
                    if (bulletsFired % 50 == 0)
                    {
                        int activeBullets = Object.FindObjectsOfType<Bullet>().Length;
                        Debug.Log($"[{elapsed:F2}s] Fired: {bulletsFired} | Active bullets: {activeBullets} | Fire rate: {bulletsFired / elapsed:F1}/s");
                    }
                }

                yield return null; // Fire as fast as possible
                elapsed += Time.deltaTime;
            }

            // Final statistics
            int finalBulletCount = Object.FindObjectsOfType<Bullet>().Length;
            float actualFireRate = bulletsFired / testDuration;
            float successRate = (bulletsFired / (float)fireAttempts) * 100f;

            Debug.Log($"=== EXTREME FIRE RATE STRESS TEST COMPLETE ===");
            Debug.Log($"Target fire rate: {targetFireRate} rounds/second");
            Debug.Log($"Actual fire rate: {actualFireRate:F1} rounds/second");
            Debug.Log($"Total bullets fired: {bulletsFired}");
            Debug.Log($"Fire attempts: {fireAttempts}");
            Debug.Log($"Success rate: {successRate:F1}%");
            Debug.Log($"Bullets still active: {finalBulletCount}");

            // Assertions
            Assert.Greater(bulletsFired, 100, "Should fire at least 100 bullets in 3 seconds");
            Assert.IsNotNull(projectileWeapon, "Weapon should survive extreme fire rate");
            Assert.Greater(actualFireRate, 30f, "Should maintain at least 30 rounds/sec average");
        }

        /// <summary>
        /// STRESS TEST: Rapid Fire with Direction Changes
        /// Watch in Editor: Bullets fire in all directions while spawning rapidly
        /// Duration: 5 seconds, changing fire direction continuously
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_RapidFireWithDirectionChanges()
        {
            Debug.Log("=== STARTING RAPID FIRE + DIRECTION CHANGE STRESS TEST ===");
            Debug.Log("Watch: Bullets will fire in random directions at high rate");

            // Configure weapon for rapid fire
            projectileWeapon.fireRate = 50f; // 50 rounds per second

            float testDuration = 5f;
            float elapsed = 0f;
            int bulletsFired = 0;
            float directionChangeInterval = 0.1f; // Change direction every 0.1s
            float nextDirectionChange = 0f;
            Vector3 currentDirection = Vector3.forward;

            while (elapsed < testDuration)
            {
                // Change fire direction periodically
                if (elapsed >= nextDirectionChange)
                {
                    // Random direction on a sphere
                    float angleH = Random.Range(0f, 360f);
                    float angleV = Random.Range(-30f, 30f); // Mostly horizontal
                    currentDirection = Quaternion.Euler(angleV, angleH, 0) * Vector3.forward;
                    nextDirectionChange = elapsed + directionChangeInterval;

                    Debug.Log($"[{elapsed:F2}s] Direction changed to {currentDirection}");
                }

                // Try to fire
                Vector3 fireOrigin = projectileWeapon.muzzle.position;
                bool fired = projectileWeapon.TryFire(fireOrigin, currentDirection);

                if (fired)
                {
                    bulletsFired++;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            int activeBullets = Object.FindObjectsOfType<Bullet>().Length;
            float avgFireRate = bulletsFired / testDuration;

            Debug.Log($"=== RAPID FIRE + DIRECTION STRESS TEST COMPLETE ===");
            Debug.Log($"Bullets fired: {bulletsFired}");
            Debug.Log($"Average fire rate: {avgFireRate:F1} rounds/second");
            Debug.Log($"Active bullets: {activeBullets}");
            Debug.Log($"Direction changes: {Mathf.FloorToInt(testDuration / directionChangeInterval)}");

            Assert.Greater(bulletsFired, 50, "Should fire many bullets with direction changes");
            Assert.IsNotNull(projectileWeapon, "Weapon should survive rapid fire with direction changes");
        }

        /// <summary>
        /// STRESS TEST: Maximum Fire Rate Limit Test
        /// Watch in Editor: Tests the absolute maximum fire rate the weapon can achieve
        /// This will attempt to fire EVERY frame
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_MaximumFireRateLimit()
        {
            Debug.Log("=== STARTING MAXIMUM FIRE RATE LIMIT TEST ===");
            Debug.Log("Testing absolute maximum achievable fire rate");

            // Set absurdly high fire rate to test the limit
            projectileWeapon.fireRate = 1000f; // 1000 rounds per second (impossible)

            float testDuration = 2f;
            float elapsed = 0f;
            int bulletsFired = 0;
            int frameCount = 0;

            while (elapsed < testDuration)
            {
                Vector3 fireOrigin = projectileWeapon.muzzle.position;
                Vector3 fireDirection = Vector3.forward;

                bool fired = projectileWeapon.TryFire(fireOrigin, fireDirection);

                if (fired)
                {
                    bulletsFired++;
                }

                frameCount++;
                yield return null;
                elapsed += Time.deltaTime;
            }

            float actualMaxRate = bulletsFired / testDuration;
            float framesPerSecond = frameCount / testDuration;
            float bulletsPerFrame = bulletsFired / (float)frameCount;

            Debug.Log($"=== MAXIMUM FIRE RATE TEST COMPLETE ===");
            Debug.Log($"Configured fire rate: 1000 rounds/second");
            Debug.Log($"Actual maximum rate: {actualMaxRate:F1} rounds/second");
            Debug.Log($"Average FPS: {framesPerSecond:F1}");
            Debug.Log($"Bullets per frame: {bulletsPerFrame:F3}");
            Debug.Log($"Total frames: {frameCount}");
            Debug.Log($"Total bullets: {bulletsFired}");

            Assert.Greater(bulletsFired, 10, "Should fire at least some bullets");
            Assert.Less(actualMaxRate, 200f, "Fire rate should be capped by Unity's frame rate");
            Debug.Log($"Fire rate properly limited: {actualMaxRate:F1} < 200 rounds/sec");
        }

        #endregion

        #region Combined Stress Tests

        /// <summary>
        /// STRESS TEST: Movement + Rapid Fire Combined
        /// Watch in Editor: Player moves rapidly while firing in all directions
        /// Duration: 5 seconds of chaos
        /// This is the ultimate stress test!
        /// </summary>
        [UnityTest]
        public IEnumerator StressTest_MovementAndRapidFire_Combined()
        {
            Debug.Log("=== STARTING ULTIMATE COMBINED STRESS TEST ===");
            Debug.Log("Watch: Player moves + rotates + fires rapidly = CHAOS!");

            projectileWeapon.fireRate = 60f; // 60 rounds per second

            float testDuration = 5f;
            float elapsed = 0f;
            int bulletsFired = 0;
            int movementActions = 0;

            Vector3 startPosition = playerObject.transform.position;

            while (elapsed < testDuration)
            {
                // Random movement
                float moveX = Random.Range(-1f, 1f);
                float moveZ = Random.Range(-1f, 1f);
                Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

                if (moveDir.magnitude > 0.1f)
                {
                    Vector3 motion = moveDir * playerController.sprintSpeed * Time.deltaTime;
                    characterController.Move(motion);
                    playerObject.transform.rotation = Quaternion.LookRotation(moveDir);
                    movementActions++;
                }

                // Random rotation (simulate mouse look)
                float randomYaw = Random.Range(-5f, 5f);
                playerController.cam.Rotate(0, randomYaw, 0);

                // Fire weapon
                Vector3 fireOrigin = projectileWeapon.muzzle.position;
                Vector3 fireDirection = playerController.cam.forward;

                bool fired = projectileWeapon.TryFire(fireOrigin, fireDirection);
                if (fired) bulletsFired++;

                // Log progress
                if (elapsed > 0 && Mathf.FloorToInt(elapsed) != Mathf.FloorToInt(elapsed - Time.deltaTime))
                {
                    int activeBullets = Object.FindObjectsOfType<Bullet>().Length;
                    Debug.Log($"[{Mathf.FloorToInt(elapsed)}s] Bullets: {bulletsFired} | Moves: {movementActions} | Active: {activeBullets}");
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            float distanceMoved = Vector3.Distance(startPosition, playerObject.transform.position);
            int finalBulletCount = Object.FindObjectsOfType<Bullet>().Length;

            Debug.Log($"=== ULTIMATE STRESS TEST COMPLETE ===");
            Debug.Log($"Bullets fired: {bulletsFired} ({bulletsFired / testDuration:F1}/s)");
            Debug.Log($"Movement actions: {movementActions}");
            Debug.Log($"Distance traveled: {distanceMoved:F2}m");
            Debug.Log($"Active bullets: {finalBulletCount}");
            Debug.Log($"Player survived: {playerObject != null}");
            Debug.Log($"Weapon survived: {projectileWeapon != null}");

            // Final assertions
            Assert.IsNotNull(playerObject, "Player should survive combined stress");
            Assert.IsNotNull(projectileWeapon, "Weapon should survive combined stress");
            Assert.Greater(bulletsFired, 100, "Should fire many bullets");
            Assert.Greater(distanceMoved, 1f, "Player should have moved");

            Debug.Log("=== ALL SYSTEMS SURVIVED ULTIMATE STRESS TEST ===");
        }

        #endregion
    }
}
