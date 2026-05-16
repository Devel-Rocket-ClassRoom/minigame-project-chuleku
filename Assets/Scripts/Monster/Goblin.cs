using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
public class Goblin : DamageAble
{
    private InputAction inputActions;
    private Animator animator;
    private bool dieCheck;
    void Awake()
    {
        dieCheck = false;
        animator = GetComponent<Animator>();
        inputActions = InputSystem.actions.FindAction("Player/Jump");
    }
    private void OnEnable()
    {
        health = 100f;
        defense = 5;
        type = Type.Minion;
    }   
    public override void Die()
    {
        if (dieCheck) return;
        dieCheck = true;
        animator.SetTrigger("Die"); 
    }
     void Update()
    {
        OnDamage();
    }
    void OnDamage()
    {
        if(inputActions.WasPressedThisFrame())
        {
            TakeDamage(20f);
        }
    }
    public void AnimationDestroy()
    {
        Destroy(gameObject);
    }
}
