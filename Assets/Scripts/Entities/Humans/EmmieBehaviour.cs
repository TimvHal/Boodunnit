using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entities.Humans
{
    public class EmmieBehaviour : BaseEntity
    {
        public bool IsCrying;

        [SerializeField] private Animator _animator;

        //private Animator _animator;
        
        private void Awake()
        {
            InitBaseEntity();
            _animator = GetComponentInChildren<Animator>();
            CanJump = false;
        }

        private void LateUpdate()
        {
            _animator.SetBool("IsCrying", IsCrying);
        }

        public override void UseFirstAbility()
        {
            //Todo throw plant pot on Julia's head.
        }

        public void TalkWithBoolia()
        {
            if (SceneManager.GetActiveScene().name.Equals("CemeteryScene_1") && IsCrying)
            {
                StopCrying();
                ActivateTransitionTrigger();
            }
        }

        private void StopCrying()
        {
            IsCrying = false;
        }

        private void ActivateTransitionTrigger()
        {
            GameObject.Find("TransitionToCrimeScene").GetComponent<BoxCollider>().enabled = true; 
        }
    }
}