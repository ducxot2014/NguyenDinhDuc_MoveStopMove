public interface IAttackRange
{
   
        void EnlargeBy(float size);
        void SetDefaultScale(float scale);
        void ResetAttack();
        void ShootAtTarget(Character target);
}
