using UnityEngine;
using UnityEngine.Audio;

public class FightAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] hitAudioClips;

    [SerializeField] private AudioClip[] maleHurtAudioClips;
    [SerializeField] private AudioClip[] femaleHurtAudioClips;

    [SerializeField] private float minDistance = 20f;
    [SerializeField] private float maxDistance = 100f;

    protected override void Subscribe()
    {
        base.Subscribe();

        AttackComponent.OnGlobalAttacked += OnAttacked;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        AttackComponent.OnGlobalAttacked -= OnAttacked;
    }

    private void OnAttacked(AttackComponent attackComponent)
    {
        AudioUtils.PlaySFXAtPosition(hitAudioClips, attackComponent.transform.position, minDistance, maxDistance, mixerGroup);

        var hurtAudio = attackComponent.GetComponent<GenderComponent>().IsMale ? maleHurtAudioClips : femaleHurtAudioClips;
        AudioUtils.PlaySFXAtPosition(hurtAudio, attackComponent.transform.position, minDistance, maxDistance, mixerGroup);
    }
}