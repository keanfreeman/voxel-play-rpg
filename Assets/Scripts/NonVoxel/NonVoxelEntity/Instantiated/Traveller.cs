using DieNamespace;
using EntityDefinition;
using GameMechanics;
using MovementDirection;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelPlay;
using static RandomManager;
using static UnityEngine.GraphicsBuffer;

namespace Instantiated {
    public abstract class Traveller : TangibleEntity {
        [SerializeField] public VoxelPlayLight vpLight;
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected CameraManager cameraManager;
        [SerializeField] protected PartyManager partyManager;
        [SerializeField] protected FeatureManager featureManager;
        [SerializeField] protected RandomManager randomManager;
        [SerializeField] protected VisualRollManager visualRollManager;
        [SerializeField] protected StatusUIController statusUIController;
        [SerializeField] protected HealthBarController healthBarController;
        private TimerUIController timerUIController;
        private CombatManager combatManager;
        private EffectManager effectManager;

        public event System.Func<Traveller, Damage, IEnumerator> onTakeDamage;
        public event System.Func<Traveller, Traveller, Advantage, Advantage> onAttackRollStart;
        public event System.Action<Traveller> onAttackRollFinished;
        public event System.Func<Traveller, IEnumerator> onCombatTurnStart;
        public event System.Func<Traveller, IEnumerator> onCombatTurnEnd;
        public event System.Func<Traveller, Advantage, IEnumerator> onAttackHit;
        public event System.Func<ActionSO, Traveller, IEnumerator> onDealDamage;

        public SpriteMoveDirection permanentMoveDirection { get; protected set; } = SpriteMoveDirection.NONE;
        public bool isMoving { get; protected set; } = false;
        public TravellerIdentitySO travellerIdentity { get; protected set; }

        protected Vector3Int moveStartPoint;
        protected Vector3Int moveEndPoint;
        protected float moveStartTimestamp;
        private float moveFinishedTimestamp;

        protected const float TIME_TO_MOVE_A_TILE = 0.2f;
        private const float ANIMATION_COOLDOWN_TIME = 0.15f;

        public int CurrHP {
            get {return GetEntity().currHP;}
        }

        // todo - break this entirely out of traveller
        public CurrentStatus GetStatusEffects() {
            return GetEntity().statusEffects;
        }

        private void Update() {
            AnimateMove();
            if (!isMoving && Time.time - moveFinishedTimestamp > ANIMATION_COOLDOWN_TIME) {
                SetMoveAnimation(false);
                moveFinishedTimestamp = float.MaxValue;
            }
        }

        protected void Init(TimerUIController timerUIController, CombatManager combatManager,
                EffectManager effectManager) {
            this.timerUIController = timerUIController;
            this.combatManager = combatManager;
            this.effectManager = effectManager;

            if (GetEntity().currHP < 0) {
                SetHP(GetStats().maxHP);
            }

            statusUIController.SetStatuses(GetStatusEffects());
            if (GetStatusEffects().NumStatuses() > 0) {
                timerUIController.OnSecondsPassed += HandleTimeDeductedForStatuses;
            }
        }

        public bool HasCondition(Condition condition) {
            return GetStatusEffects().GetConditions().Contains(condition);
        }

        public OngoingEffect GetStatus(StatusEffect statusEffect) {
            return GetStatusEffects().Get(statusEffect);
        }

        public void AddStatus(OngoingEffect ongoingEffect) {
            // todo - move to status manager
            if (ongoingEffect.cause == StatusEffect.Light) {
                vpLight.enabled = true;
            }

            if (GetStatusEffects().NumStatuses() < 1) {
                timerUIController.OnSecondsPassed += HandleTimeDeductedForStatuses;
            }
            // todo, unify these controls
            GetStatusEffects().Set(ongoingEffect.cause, ongoingEffect);
            statusUIController.SetStatuses(GetStatusEffects());
        }

        public bool HasStatus(StatusEffect statusEffect) {
            return GetStatusEffects().ongoingEffects.ContainsKey(statusEffect);
        }

        public void RemoveStatus(StatusEffect statusEffect, bool rerender = true) {
            // todo - move to status manager
            if (statusEffect == StatusEffect.Light) {
                vpLight.enabled = false;
            }

            GetStatusEffects().Remove(statusEffect);
            if (GetStatusEffects().NumStatuses() < 1) {
                timerUIController.OnSecondsPassed -= HandleTimeDeductedForStatuses;
            }
            if (rerender) statusUIController.SetStatuses(GetStatusEffects());
        }

        private void HandleTimeDeductedForStatuses(int secondsDeducted) {
            List<StatusEffect> toRemove = GetStatusEffects().DeductTime(secondsDeducted);
            if (toRemove.Count < 1) {
                return;
            }
            foreach (StatusEffect statusEffect in toRemove) {
                RemoveStatus(statusEffect, rerender: false);
            }
            statusUIController.SetStatuses(GetStatusEffects());
            if (GetStatusEffects().NumStatuses() < 1) {
                timerUIController.OnSecondsPassed -= HandleTimeDeductedForStatuses;
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
            if (onAttackRollStart != null) {
                foreach (System.Delegate handler in onAttackRollStart.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, Traveller, Advantage, Advantage>)handler;
                    Advantage nextAdvantage = handlerCasted.Invoke(this, target, currAdvantageState);
                    currAdvantageState = AdvantageCalcs.GetNewAdvantageState(currAdvantageState, nextAdvantage);
                }
            }

            CoroutineWithData<AttackResult> rollAttackCoroutine = new(this, 
                visualRollManager.RollAttack(this, attack.attackRoll.modifier,
                target.GetStats().CalculateArmorClass(GetStatusEffects()), currAdvantageState));
            yield return rollAttackCoroutine.coroutine;
            AttackResult attackResult = rollAttackCoroutine.GetResult();

            onAttackRollFinished?.Invoke(this);
            yield return attackResult;
        }

        public IEnumerator RecoverHP(int healingAmount) {
            if (CurrHP < 1) {
                RemoveStatus(StatusEffect.KnockedOut);
                animator.SetBool("isFrozen", false);
            }
            SetHP(Mathf.Min(GetStats().maxHP, CurrHP + healingAmount));
            // todo play healing effect and display heal amount
            yield break;
        }

        public IEnumerator DealDamage(ActionSO damageAction, Damage damage, Traveller target) {
            yield return target.TakeDamage(damage);

            if (onDealDamage != null) {
                foreach (System.Delegate handler in onDealDamage.GetInvocationList()) {
                    var handlerCasted = (System.Func<ActionSO, Traveller, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(damageAction, target);
                }
            }
        }

        // todo - particle effect for amount taken
        private IEnumerator TakeDamage(Damage damage) {
            yield return effectManager.GenerateHitEffect(this);
            int newHP = GetEntity().currHP - damage.amount;
            newHP = newHP < 0 ? 0 : newHP;
            SetHP(newHP);

            if (onTakeDamage != null) {
                foreach (System.Delegate handler in onTakeDamage.GetInvocationList()) {
                    var handlerCasted = (System.Func<Traveller, Damage, IEnumerator>)handler;
                    yield return handlerCasted.Invoke(this, damage);
                }
            }

            if (CurrHP < 1) {
                if (GetType() == typeof(PlayerCharacter)) {
                    AddStatus(new OngoingEffect(StatusEffect.KnockedOut,
                        new HashSet<Condition> { Condition.Unconscious }, int.MaxValue));
                    animator.SetBool("isFrozen", true);
                }
                else {
                    // to avoid issue of destroying something whose code is running
                    StartCoroutine(combatManager.DestroyCombatant(this));
                }
            }
        }

        public void SetHP(int newValue) {
            GetEntity().currHP = newValue;
            healthBarController.SetHealth(GetEntity().currHP, GetStats().maxHP);
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
            if (!isMoving && !cameraManager.isRotating 
                    && permanentMoveDirection != SpriteMoveDirection.NONE) {
                if (HasCondition(Condition.Paralyzed) || HasCondition(Condition.Unconscious)) {
                    return;
                }

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

        public void MoveOriginImmediately(Vector3Int point) {
            nonVoxelWorld.RemovePositions(this);
            MoveOccupiedPositionsTo(point);
            nonVoxelWorld.SetPositions(this);
            transform.position = point;
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

        public CurrentResources GetResources() {
            return GetEntity().resources;
        }

        public void InitResources(List<Resource> resourceDefinitions) {
            GetEntity().resources = new(resourceDefinitions);
        }
    }
}
