using UnityEngine;

public sealed class PoolAudio : MonoBehaviour
{
    AudioSource _src;
    AudioClip _shot;
    AudioClip _click;
    AudioClip _win;

    public void Build()
    {
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _shot = Tone(220, 0.08f, 0.35f);
        _click = Tone(520, 0.04f, 0.2f);
        _win = Tone(440, 0.25f, 0.25f);
    }

    public void PlayShot() => _src.PlayOneShot(_shot, 0.7f);
    public void PlayClick() => _src.PlayOneShot(_click, 0.5f);
    public void PlayWin() => _src.PlayOneShot(_win, 0.8f);

    static AudioClip Tone(float hz, float dur, float vol)
    {
        var freq = 44100;
        var n = Mathf.CeilToInt(freq * dur);
        var clip = AudioClip.Create("tone", n, 1, freq, false);
        var data = new float[n];
        for (var i = 0; i < n; i++)
        {
            var t = i / (float)freq;
            var env = 1f - t / dur;
            data[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * vol * env;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
