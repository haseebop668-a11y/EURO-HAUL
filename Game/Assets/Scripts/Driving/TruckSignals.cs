using UnityEngine;
using UnityEngine.Events;
using EuroHaul.Traffic;

namespace EuroHaul.Driving
{
    public enum TurnSignalState { Off, Left, Right, Hazard }

    /// <summary>Driver-controlled exterior indicators, hazard lamps and horn.</summary>
    public sealed class TruckSignals : MonoBehaviour
    {
        [SerializeField] private Renderer leftIndicator;
        [SerializeField] private Renderer rightIndicator;
        [SerializeField] private AudioSource hornAudio;
        [SerializeField] private TrafficHornBroadcast hornBroadcast;
        [SerializeField, Min(0.1f)] private float blinkInterval = 0.45f;
        [SerializeField] private string emissionColorProperty = "_EmissionColor";
        [SerializeField] private Color activeColor = new Color(1f, 0.35f, 0.02f);
        [SerializeField] private Color inactiveColor = Color.black;
        [SerializeField] private UnityEvent onHorn;

        private MaterialPropertyBlock leftBlock;
        private MaterialPropertyBlock rightBlock;
        private float nextBlink;
        private bool blinkOn;
        private bool hornHeld;

        public TurnSignalState State { get; private set; }
        public bool IsHornHeld => hornHeld;
        public bool IsBlinkOn => blinkOn;

        private void Awake()
        {
            if (hornBroadcast == null) hornBroadcast = GetComponent<TrafficHornBroadcast>();
            leftBlock = new MaterialPropertyBlock();
            rightBlock = new MaterialPropertyBlock();
            ApplyLamps(false, false);
        }

        private void Update()
        {
            if (State == TurnSignalState.Off || Time.time < nextBlink) return;
            blinkOn = !blinkOn;
            nextBlink = Time.time + blinkInterval;
            ApplyLamps(
                blinkOn && (State == TurnSignalState.Left || State == TurnSignalState.Hazard),
                blinkOn && (State == TurnSignalState.Right || State == TurnSignalState.Hazard));
        }

        public void ToggleLeft() => SetState(State == TurnSignalState.Left ? TurnSignalState.Off : TurnSignalState.Left);
        public void ToggleRight() => SetState(State == TurnSignalState.Right ? TurnSignalState.Off : TurnSignalState.Right);
        public void ToggleHazard() => SetState(State == TurnSignalState.Hazard ? TurnSignalState.Off : TurnSignalState.Hazard);
        public void CancelSignal() => SetState(TurnSignalState.Off);

        public void SetState(TurnSignalState state)
        {
            State = state;
            blinkOn = state != TurnSignalState.Off;
            nextBlink = Time.time + blinkInterval;
            ApplyLamps(blinkOn && (state == TurnSignalState.Left || state == TurnSignalState.Hazard),
                       blinkOn && (state == TurnSignalState.Right || state == TurnSignalState.Hazard));
        }

        public void HornDown()
        {
            if (hornHeld) return;
            hornHeld = true;
            if (hornAudio != null && !hornAudio.isPlaying) hornAudio.Play();
            onHorn?.Invoke();
            if (hornBroadcast != null) hornBroadcast.BroadcastHorn();
        }

        public void HornUp()
        {
            hornHeld = false;
            if (hornAudio != null && hornAudio.isPlaying) hornAudio.Stop();
        }

        private void ApplyLamps(bool leftOn, bool rightOn)
        {
            SetEmission(leftIndicator, leftBlock, leftOn);
            SetEmission(rightIndicator, rightBlock, rightOn);
        }

        private void SetEmission(Renderer target, MaterialPropertyBlock block, bool enabled)
        {
            if (target == null) return;
            target.GetPropertyBlock(block);
            block.SetColor(emissionColorProperty, enabled ? activeColor : inactiveColor);
            target.SetPropertyBlock(block);
        }

        private void OnDisable()
        {
            hornHeld = false;
            if (hornAudio != null && hornAudio.isPlaying) hornAudio.Stop();
            SetState(TurnSignalState.Off);
        }
    }
}