using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Interfaces;
using UnityEngine;

namespace Entities
{
    public class Dog : MonoBehaviour, IAnimal
    {
        public float FearThreshold { get; set; }
        public float FearDamage { get; set; }
        public float FaintDuration { get; set; }
        public EmotionalState EmotionalState { get; set; }
        public Dictionary<Type, float> ScaredOfGameObjects { get; set; }
        
        public void DealFearDamage(float amount)
        {
            throw new NotImplementedException();
        }

        public IEnumerator CalmDown()
        {
            throw new System.NotImplementedException();
        }

        public void Faint()
        {
            throw new System.NotImplementedException();
        }

        public void CheckSurroundings()
        {
            throw new System.NotImplementedException();
        }

        public void UseFirstAbility()
        {
            throw new System.NotImplementedException();
        }

        public void UseSecondAbility()
        {
            throw new System.NotImplementedException();
        }
    }
}