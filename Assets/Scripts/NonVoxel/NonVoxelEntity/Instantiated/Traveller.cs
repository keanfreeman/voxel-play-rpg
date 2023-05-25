using DieNamespace;
using EntityDefinition;
using GameMechanics;
using MovementDirection;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RandomManager;
using static UnityEngine.GraphicsBuffer;

namespace Instantiated {
    public abstract class Traveller : TangibleEntity {
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected CameraManager cameraManager;
        [SerializeField] protected PartyManager partyManager;
        [SerializeField] protected FeatureManager featureManager;
        [SerializeField] protected RandomManager randomManager;
        [SerializeField] protected VisualRollManager visualRollManager;

        public event System.Func<Traveller, Damage, IEnumerator> onHPChanged;
        public event System.Func<Traveller, Traveller, Advantage, Advantage> onPerformAttack;
        public event System.Func<Traveller, IEnumerator> onCombatTurnStart;
        public event System.Func<Traveller, IEnumerator> onCombatTurnEnd;
        public event System.Func<Traveller, Advantage, IEnumerator> onAttackHit;
        public event System.Func<AttackSO, Traveller, IEnumerator> afterDamageDealt;

        public SpriteMoveDirection permanentMoveDirection { get; protected set; } = SpriteMoveDirection.NONE;
        public bool isMoving { get; protected set; } = false;

        protected TravellerIdentitySO travellerIdentity;
        protected Vector3Int moveStartPoint;
        protected Vector3Int moveEndPoint;
        protected float moveStartTimestamp;
        private float moveFinishedTimestamp;

        protected const float TIME_TO_MOVE_A_TILE = 0.2f;
        private const float ANIMATION_COOLDOWN_TIME = 0.1f;

        public int CurrHP {
            get {return GetEntity().currHP;}
            set { GetEntity().currHP = value; }
        }

        public CurrentStatus StatusEffects {
            get { return GetEntity().statusEffects; }
        }

        private void Update() {
            AnimateMove();
            if (!isMoving && Time.time - moveFinishedTimestamp > ANIMATION_COOLDOWN_TIME) {
                SetMoveAnimation(false);
                moveFinishedTimestamp = float.MaxValue;
            }
        }

        protected void Init() {
            if (GetEntity().currHP < 0) {
                GetEntity().currHP = GetStats().maxHP;
            }
        }

        public IEnumerator PerformDamage(AttackSO attack, AttackResult attackResult, Traveller target) {
            List<Die> bonusDamage = new();
            bool isCurrentlyCritical = attackResult.isCritical;
            if (onAttackHit != null) {
                foreach (System.Func<Traveller, Advantage, IEnumerator> handler 
                        in onAttackHit.GetInvocationList()) {
                    CoroutineWithData<AttackHitModifications> attackHitCoroutine = new(this,
                        handler.Invoke(target, attackResult.advantageState));
                    yield return attackHitCoroutine.coroutine;

                    AttackHitModifications modifications = attackHitCoroutine.GetResult();
                    if (modifications.isNewlyCritical) isCurrentlyCritical = true;
                    if (modifications.bonusDamage.Count > 0) bonusDamage.AddRange(modifications.bonusDamage);
                }
            }

            List<Die> finalDamage = new() { attack.damageRoll };
            finalDamage.AddRange(bonusDamage);
            CoroutineWithData<int> damageCoroutine = new(this, 
                visualRollManager.RollDamage(finalDamage, isCurrentlyCritical));
            yield return damageCoroutine.coroutine;
            yield return damageCoroutine.GetResult();
        }

        public IEnumerator PerformAttack(AttackSO attack, Traveller target, 
                Advantage advantage = Advantage.None) {
            Advantage currAdvantageState = advantage;
            if (onPerformAttack != null) {
                foreach (System.Delegate handler in onPerformAttack.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, Traveller, Advantage, Advantage>)handler;
                    currAdvantageState = handlerCasted.Invoke(this, target, currAdvantageState);
                }
            }

            CoroutineWithData<AttackResult> rollAttackCoroutine = new(this, 
                visualRollManager.RollAttack(attack.attackRoll.modifier,
                target.GetStats().CalculateArmorClass(), currAdvantageState));
            yield return rollAttackCoroutine.coroutine;
            AttackResult attackResult = rollAttackCoroutine.GetResult();
            yield return attackResult;
        }

        public IEnumerator DealDamage(AttackSO attack, Damage damage, Traveller target) {
            yield return target.TakeDamage(damage);

            if (afterDamageDealt != null) {
                foreach (System.Delegate handler in afterDamageDealt.GetInvocationList()) {
                    var handlerCasted = (System.Func<AttackSO, Traveller, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(attack, target);
                }
            }
        }

        private IEnumerator TakeDamage(Damage damage) {
            GetEntity().currHP -= damage.amount;

            if (onHPChanged != null) {
                foreach (System.Delegate handler in onHPChanged.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, Damage, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(this, damage);
                }
            }
        }

        public void SetHP(int newValue) {
            GetEntity().currHP = newValue;
        }

        public IEnumerator OnCombatTurnStart() {
            if (onCombatTurnStart != null) {
                foreach (var handler in onCombatTurnStart.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(this);
                }
            }
        }

        public IEnumerator OnCombatTurnEnd() {
            if (onCombatTurnEnd != null) {
                foreach (var handler in onCombatTurnEnd.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(this);
                }
            }
        }

        private void AnimateMove() {
            if (!isMoving && !cameraManager.isRotating && permanentMoveDirection != SpriteMoveDirection.NONE) {
                Vector3Int? direction = GetDestinationFromDirection(permanentMoveDirection);
                if (direction.HasValue) {
                    MoveOriginToPoint(direction.Value);
                }
            }
            if (isMoving) {
                float timeSinceMoveBegan = Time.time - moveStartTimestamp;
                float fractionOfMovementDone = Mathf.Min(timeSinceMoveBegan / (TIME_TO_MOVE_A_TILE), 1f);
                transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint, fractionOfMovementDone);

                if (fractionOfMovementDone >= 1f) {
                    moveFinishedTimestamp = Time.time;
                    isMoving = false;
                }
            }
        }

        public void MoveOriginToPoint(Vector3Int point) {
            moveStartPoint = origin;

            nonVoxelWorld.RemovePositions(this);
            MoveOccupiedPositionsTo(point);
            nonVoxelWorld.SetPositions(this);

            moveEndPoint = point;

            moveStartTimestamp = Time.time;
            isMoving = true;
            SetMoveAnimation(isMoving);
            SetMoveDirectionRelativeToCamera();

            if (partyManager.currControlledCharacter == this) {
                partyManager.OnLeaderMoved(moveStartPoint);
            }
        }

        public void SetMoveAnimation(bool state) {
            animator.SetBool("isMoving", state);
        }

        public bool IsAnimatingMove() {
            return animator.GetBool("isMoving");
        }

        private void SetMoveDirectionRelativeToCamera() {
            Direction cameraDirection = cameraManager.GetCameraApproximateDirection();
            Vector3Int diff = moveEndPoint - moveStartPoint;
            Direction absoluteCreatureMove = diff.z > 0 ? Direction.NORTH :
                diff.z < 0 ? Direction.SOUTH :
                diff.x > 0 ? Direction.EAST :
                Direction.WEST;
            if ((cameraDirection == Direction.NORTH && absoluteCreatureMove == Direction.EAST)
                    || (cameraDirection == Direction.EAST && absoluteCreatureMove == Direction.SOUTH)
                    || (cameraDirection == Direction.SOUTH && absoluteCreatureMove == Direction.WEST)
                    || (cameraDirection == Direction.WEST && absoluteCreatureMove == Direction.NORTH)) {
                spriteRenderer.flipX = true;
            }
            else if ((cameraDirection == Direction.NORTH && absoluteCreatureMove == Direction.WEST)
                    || (cameraDirection == Direction.EAST && absoluteCreatureMove == Direction.NORTH)
                    || (cameraDirection == Direction.SOUTH && absoluteCreatureMove == Direction.EAST)
                    || (cameraDirection == Direction.WEST && absoluteCreatureMove == Direction.SOUTH)) {
                spriteRenderer.flipX = false;
            }
        }

        public abstract void RotateSprite(float degrees);

        public StatsSO GetStats() {
            return travellerIdentity.stats;
        }

        public new EntityDefinition.Traveller GetEntity() {
            return (EntityDefinition.Traveller)entity;
        }

        protected abstract Vector3Int? GetDestinationFromDirection(SpriteMoveDirection spriteMoveDirection);

        public abstract Faction GetFaction();

        public List<ActionSO> GetActions() {
            List<ActionSO> totalActions = new();
            StatsSO stats = GetStats();
            foreach (ActionSO action in stats.actions) {
                totalActions.Add(action);
            }
            List<FeatureSO> features = stats.features;
            foreach (FeatureSO feature in features) {
                foreach (ActionSO action in feature.providedActions) {
                    totalActions.Add(action);
                }
            }

            return totalActions;
        }

        public List<Resource> GetConsumedResources() {
            return GetEntity().consumedResources;
        }

        public void AddConsumedResource(Resource consumed) {
            GetEntity().consumedResources.Add(consumed);
        }
    }
}
