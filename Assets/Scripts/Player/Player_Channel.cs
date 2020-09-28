using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class Player_Channel : MonoBehaviour, ITimeBody
{
    private Planet planet;
    private Player player;

    [SerializeField]
    private Image image;

    private void Start()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();

        player.SwallowedOther += Player_SwallowedOther;
        player.WasSwallowed += Player_WasSwallowed;
    }

    private void Player_SwallowedOther(object sender, System.EventArgs e)
    {
        // TODO:
    }

    private void Player_WasSwallowed(object sender, System.EventArgs e)
    {
        SetImageAlpha(0.15f);

        if (player.HasCurrentActiveCamera())
        {
            // TODO: game over?
        }
    }

    private float GetImageAlpha()
    {
        return image != null ? image.color.a : 0.0f;        
    }

    private void SetImageAlpha(float value)
    {
        if (image != null)
        {
            var c = image.color;
            c.a = value;
            image.color = c;
        }
    }

    void ITimeBody.RewindStarted() { }

    void ITimeBody.RewindStopped() { }

    IMemento ITimeBody.CreateMemento()
    {
        return new Memento
        {
            ImageAlpha = GetImageAlpha(),
        };
    }

    void ITimeBody.RestoreMemento(IMemento o)
    {
        var memento = (Memento)o;
        SetImageAlpha(memento.ImageAlpha);
    }

    private sealed class Memento : IMemento
    {
        public float ImageAlpha { get; set; }
    }
}
