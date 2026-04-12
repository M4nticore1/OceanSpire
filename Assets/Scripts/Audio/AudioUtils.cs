using UnityEngine;

public static class AudioUtils
{
    public static void PlaySFX(AudioClip clip)
    {
        if (!clip) return;

        var src = CreateAudioSource();
        src.clip = clip;
        src.spatialBlend = 0f;

        src.Play();
        Object.Destroy(src.gameObject, clip.length);
    }

    public static void PlaySFXAtPosition(AudioClip clip, Vector3 pos, float minDist, float maxDist)
    {
        if (!clip) return;

        var src = CreateAudioSource();
        src.gameObject.transform.position = pos;

        src.clip = clip;
        src.minDistance = minDist;
        src.maxDistance = maxDist;
        src.spatialBlend = 1f;
        src.dopplerLevel = 0f;

        src.Play();
        GameObject.Destroy(src.gameObject, clip.length);
    }

    public static AudioClip GetRandomAudioClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;

        int index = Random.Range(0, clips.Length);

        return clips[index];
    }

    private static AudioSource CreateAudioSource()
    {
        var go = new GameObject("SFX");
        var src = go.AddComponent<AudioSource>();
        return src;
    }
}