using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Enums;
using Enums;
using UnityEngine;

namespace Entities
{
    public abstract class BaseEntity : BaseEntityMovement, IPossessable, IIconable
    {
        //Public properties
        public bool IsPossessed { get; set; }
        public bool CanPossess = true;
        public int TimesPosessed;
        public bool HasToggledAbility;
        
        public bool IsWalking { get; set; }

        public List<AudioClip> WalkAudioClips;

        //Properties & Fields regarding Dialogue mechanic.
        [Header("Conversation")]
        public Dialogue Dialogue;
        public Question Question;
        public List<CharacterType> Relationships;
        public Sentence[] DefaultAnswers;
        public CharacterType CharacterName;
        public bool CanTalkToBoolia;

        [Header("Fear")]
        public float FearThreshold;
        public float FearDamage;
        public float FaintDuration;
        public EmotionalState EmotionalState;
        public Dictionary<CharacterType, float> ScaredOfEntities;
        public bool IsScaredOfLevitatableObject;
        public float LevitatableObjectFearDamage = 10;
        public bool HasFearCooldown;
        
        public float ParabolaHeight;

        [SerializeField] private float _fearRadius;
        [SerializeField] private float _fearAngle;
        
        [SerializeField] private RagdollController _ragdollController;

        protected void InitBaseEntity()
        {
            InitEntityMovement();
            InitVolumeForEntity();

            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            Outline outline = gameObject.AddComponent<Outline>();
            if (outline)
            {
                Color possesionColor;
                ColorUtility.TryParseHtmlString("#79957c", out possesionColor);
                outline.OutlineColor = possesionColor;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineWidth = 5.0f;
                outline.enabled = false;
            }
        }

        private void Update()
        {
            EntityWalkAnimation();
            Rigidbody.isKinematic = !IsPossessed;
            if (!IsPossessed)
            {
                CheckSurroundings();
                if (EmotionalState != EmotionalState.Fainted && FearDamage <= 0 && NavMeshAgent && NavMeshAgent.enabled && !NavMeshAgent.isStopped)
                {
                    if (_pathFindingState.Equals(PathFindingState.Following) || _pathFindingState.Equals(PathFindingState.PatrolAreas) || _pathFindingState.Equals(PathFindingState.Patrolling))
                        CheckWhichAudioClipToPlayForEntity();

                    MoveWithPathFinding();
                }
            }
            
            NavMeshAgent.autoTraverseOffMeshLink = (OffMeshLinkTraverseType == OffMeshLinkMethod.None);
            if (OffMeshLinkTraverseType == OffMeshLinkMethod.Parabola && NavMeshAgent.isOnOffMeshLink && !IsTraversingOfMeshLink)
            {
                StartCoroutine(Parabola(NavMeshAgent));
            }
        }

        public abstract void UseFirstAbility();

        public void PlayAudioClip(int index)
        {
            if (WalkAudioClips.Count <= 0)
                return;

            AudioSource audio = GetComponent<AudioSource>();
            if (audio)
            {
                audio.clip = WalkAudioClips[index];
                if(!audio.isPlaying)
                    audio.Play();
            }
        }

        public void PlayAudioOnMovement(int index)
        {
            if (IsPossessed)
            {
                if (Rigidbody.velocity.magnitude > 0.01f)
                {
                    PlayAudioClip(index);
                }
                else
                    StopAudioClip();
            } else
            {
                if (NavMeshAgent)
                {
                    if (NavMeshAgent.velocity.magnitude > 0.01f)
                        PlayAudioClip(index);
                    else
                        StopAudioClip();
                }
            }
            
        }

        public void StopAudioClip()
        {
            AudioSource audio = GetComponent<AudioSource>();
            if (audio)
                audio.Stop();
        }

        public void PlayWalkAudioClip()
        {
            if (WalkAudioClips.Count <= 0)
                return;

            AudioSource audio = GetComponent<AudioSource>();
            int randomWalkAudioClip = UnityEngine.Random.Range(0, WalkAudioClips.Count);
            if (audio)
            {
                audio.clip = WalkAudioClips[randomWalkAudioClip];
                audio.Play();
            }
        }

        protected virtual void CheckSurroundings(Vector3 raycastStartPosition)
        {
            if (HasFearCooldown || EmotionalState == EmotionalState.Fainted || IsPossessed) return;
            StartCoroutine(ActivateCooldown());
            
            Collider[] colliders = Physics.OverlapSphere(raycastStartPosition, _fearRadius);

            List<BaseEntity> baseEntities = colliders
                .Where(collider =>
                    (collider && !collider.isTrigger) &&
                    Vector3.Dot((collider.transform.root.position - transform.position).normalized, transform.forward) * 100f >= (90f - (_fearAngle / 2f)) &&
                    collider.GetComponent<BaseEntity>() &&
                    ScaredOfEntities != null &&
                    ScaredOfEntities.ContainsKey(collider.GetComponent<BaseEntity>().CharacterName))
                .Select(e => e.GetComponent<BaseEntity>())
                .ToList();

            List<LevitateableObject> levitateables = colliders
                .Where(c =>
                    Vector3.Dot((c.transform.root.position - transform.position).normalized, transform.forward) * 100f >= (90f - (_fearAngle / 2f)) &&
                    c.GetComponent<LevitateableObject>()
                    && c.GetComponent<LevitateableObject>().State != LevitationState.NotLevitating &&
                    IsScaredOfLevitatableObject)
                .Select(l => l.GetComponent<LevitateableObject>())
                .ToList();

            if (baseEntities.Count == 0 && levitateables.Count == 0) CalmDown();
            else
            {
                foreach (BaseEntity entity in baseEntities) if(!IsPossessed) DealFearDamage(ScaredOfEntities[entity.CharacterName]);
                foreach (LevitateableObject levitateable in levitateables) if(!IsPossessed) DealFearDamage(LevitatableObjectFearDamage);
            }
        }

        protected virtual void CheckSurroundings() { CheckSurroundings(transform.position); }

        protected virtual IEnumerator ActivateCooldown()
        {
            HasFearCooldown = true;
            yield return new WaitForSeconds(0.5f);
            HasFearCooldown = false;
        }

        protected virtual void DealFearDamage(float amount)
        {
            if (EmotionalState == EmotionalState.Fainted) return;
            FearDamage += amount;

            if (FearDamage >= FearThreshold / 2)
            {
                EmotionalState = EmotionalState.Terrified;
            }
            else
            {
                EmotionalState = EmotionalState.Scared;
            }

            SetScaredStage(FearDamage >= FearThreshold / 2 && EmotionalState != EmotionalState.Fainted ? 2 : 1);
            PauseEntityNavAgent(true);

            if (FearDamage >= FearThreshold && EmotionalState != EmotionalState.Fainted) Faint();
        }

        protected virtual void Faint()
        {
            EmotionalState = EmotionalState.Fainted;
            if (_ragdollController) _ragdollController.ToggleRagdoll(true);
            CanPossess = false;
            StartCoroutine(WakeUp());
        }

        protected virtual void CalmDown()
        {
            if (FearDamage > 0) FearDamage -= FearThreshold / 20f;

            if (FearDamage == 0)
            {
                EmotionalState = EmotionalState.Calm;
            }
            else if (FearDamage < FearThreshold / 2)
            {
                EmotionalState = EmotionalState.Scared;
            }

            if (FearDamage <= 0)
            {
                if (Animator && Animator.runtimeAnimatorController != null)
                {
                    if (CheckIfParamaterExists("ScaredStage"))
                    {
                        if (Animator.GetInteger("ScaredStage") > 0 && EmotionalState != EmotionalState.Fainted)
                        {
                            if (Animator.GetCurrentAnimatorStateInfo(0).IsTag("Terrified") || Animator.GetCurrentAnimatorStateInfo(0).IsTag("Scared"))
                            {
                                SetScaredStage(0);
                                Animator.Rebind();
                                ResetDestination();
                            }
                        }
                    }
                }
                FearDamage = 0;
            }
        }

        private void EntityWalkAnimation()
        {
            if (IsPossessed)
            {
                if (Rigidbody.velocity.magnitude > 1f)
                {
                    SetWalkingAnimation(true);
                }
                else
                {
                    SetWalkingAnimation(false);
                }
            } else
            {
                if (NavMeshAgent)
                {
                    if (NavMeshAgent.velocity.magnitude > 1f)
                    {
                        SetWalkingAnimation(true);
                    }
                    else
                    {

                        SetWalkingAnimation(false);
                    }
                }
            }
        }

        private void InitVolumeForEntity()
        {
            PlayerSettings settings = SaveHandler.Instance.LoadDataContainer<PlayerSettings>();
            if(settings != null)
            {
                AudioSource source = GetComponent<AudioSource>();
                if (source)
                {
                    source.volume = ((float)settings.AudioVolume / 100);
                }
            }
        }

        public void ResetFearDamage()
        {
            SetScaredStage(0);
            SetWalkingAnimation(false);

            if(Animator != null && Animator.runtimeAnimatorController != null)
                Animator.Rebind();

            FearDamage = 0;
        }

        private void SetWalkingAnimation(bool shouldWalk)
        {
            if (Animator && Animator.runtimeAnimatorController != null)
                Animator.SetBool("IsWalking", shouldWalk);
        }

        private void SetScaredStage(int scaredStage)
        {
            if (Animator && Animator.runtimeAnimatorController != null)
                Animator.SetInteger("ScaredStage", scaredStage);
        }

        protected virtual IEnumerator WakeUp()
        {
            yield return new WaitForSeconds(FaintDuration);
            FearDamage = 0;
            EmotionalState = EmotionalState.Calm;

            if(_ragdollController)  _ragdollController.ToggleRagdoll(false);

            if (Animator && Animator.runtimeAnimatorController != null)
            {
                Animator.SetInteger("ScaredStage", 0);
                Animator.Rebind();
            }

            ResetDestination();
            CanPossess = true;
        }

        private void CheckWhichAudioClipToPlayForEntity()
        {
            switch (CharacterName)
            {
                case CharacterType.Rat:
                    PlayAudioOnMovement(0);
                    break;
                case CharacterType.Bird:
                    PlayAudioOnMovement(0);
                    break;
            }
        }

        private bool CheckIfParamaterExists(string paramaterName)
        {
            foreach(AnimatorControllerParameter param in Animator.parameters)
            {
                if (param.name.ToLower() == paramaterName.ToLower())
                    return true;
            }
            return false;
        }

        public void DealFearDamageAfterDash(int damage)
        {
            DealFearDamage(damage);
        }

        public EmotionalState GetEmotionalState()
        {
            return EmotionalState;
        }

        public bool GetCanBePossessed()
        {
            return CanPossess;
        }

        public bool GetCanTalkToBoolia()
        {
            return CanTalkToBoolia;
        }
    }
}