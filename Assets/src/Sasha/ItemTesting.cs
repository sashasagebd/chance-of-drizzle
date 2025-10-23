using UnityEngine;
using System;

public class Tests : MonoBehaviour
{
    void Start()
    {
        Debug.Log("\n Starting Consumable Boundary Tests ");
        
        // TEST 1: ADD HEAL PAST MAX (Boundary)
        GameObject go1 = new GameObject("T1"); 
        PlayerController3D p1 = go1.AddComponent<PlayerController3D>(); 
        Health h1 = go1.AddComponent<Health>(); 
        p1.HealthComponent = h1;
        Consumable heal = new Consumable("Potion", "", "Heal", 50, 0); 
        h1.maxHp = 100; 
        h1.SetHealth(99); 
        heal.Use(p1); 

        bool passed1 = Mathf.Approximately(100, h1.Current);
        Debug.Log(passed1 ? "[SUCCESS] HP stayed at 100." : $"[FAILURE] HP was {h1.Current} (Expected 100).");
        GameObject.Destroy(go1); 

        // TEST 2: ADD SPEED (Boundary)
        GameObject go2 = new GameObject("T2"); 
        PlayerController3D p2 = go2.AddComponent<PlayerController3D>();
        Consumable speed = new Consumable("Speed", "", "Speed", 2, 10); 
        float expected2 = 8.0f; 
        speed.Use(p2); 

        bool passed2 = Mathf.Approximately(expected2, p2.moveSpeed);
        Debug.Log(passed2 ? "[SUCCESS] Speed is 8." : $"[FAILURE] Speed was {p2.moveSpeed} (Expected 8).");
        GameObject.Destroy(go2); 

        // TEST 3: HEAL SPAM (Stress)
        GameObject go3 = new GameObject("T3"); 
        PlayerController3D p3 = go3.AddComponent<PlayerController3D>(); 
        Health h3 = go3.AddComponent<Health>(); 
        p3.HealthComponent = h3;
        Consumable healSpam = new Consumable("Super Potion", "", "Heal", 50, 0); 

        h3.maxHp = 100; 
        h3.SetHealth(100);
        for (int i = 0; i < 1000; i++)
        {
            healSpam.Use(p3);
        }

        bool passed3 = Mathf.Approximately(100f, h3.Current);
        Debug.Log(passed3 ? "[SUCCESS] Heal Spam: System stable, HP stayed at 100." : $"[FAILURE] Heal Spam: HP was {h3.Current} (Expected 100).");
        GameObject.Destroy(go3);
        
        Debug.Log("FINISHED TESTS");
    }
}