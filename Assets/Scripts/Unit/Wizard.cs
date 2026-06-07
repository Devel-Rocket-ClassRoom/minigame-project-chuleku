using System.Collections;
using UnityEngine;

public class Wizard : UnitBase
{
    public ParticleSystem particleSystem;

    // 공격모션 시작 후 실제 타격이 나가기까지의 시간(초). 애니메이션의 캐스트 시점에 맞춰 인스펙터에서 조절.
    // 주의: 공격 간격(=1/공속)보다 작아야 한다. 더 크면 다음 공격이 이전 타격 코루틴을 끊어 데미지가 누락된다.
    private float castDelay = 0.3f;
    private Coroutine castCo;

    // 적을 처음 포착해도 즉시 쏘지 않고 공격 간격만큼 기다린 뒤 첫 공격
    protected override bool DelayFirstAttack => true;

    protected override void OnDisable()
    {
        base.OnDisable();
        if (castCo != null) { StopCoroutine(castCo); castCo = null; } // 풀 반환 시 진행 중 캐스트 정리
    }

    protected override void Attack(DamageAble target)
    {
        animator?.SetTrigger("Attack");

        // 데미지/파티클은 공격모션 시작 후 castDelay 뒤에 들어간다.
        // 기존 OnStateExit 방식은 상태가 다음 공격/Idle 전이 때까지 안 나가서 타격이 한참 늦게 터졌다.
        if (castCo != null) StopCoroutine(castCo);
        castCo = StartCoroutine(CastRoutine());
    }

    private IEnumerator CastRoutine()
    {
        yield return new WaitForSeconds(castDelay);
        castCo = null;
        TakeDamageOn();
    }

    public void TakeDamageOn()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            particleSystem.Play();
        }
        var list = sensor.Targets;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var d = list[i];
            if (d == null || d.isDead) { list.RemoveAt(i); continue; }
            d.TakeDamage(damage);
        }
        SoundManager.Play("WizardAttack");
    }
}
