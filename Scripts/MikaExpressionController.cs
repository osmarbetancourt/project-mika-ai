using UnityEngine;
using UniVRM10;
using System.Collections;

public class MikaExpressionController : MonoBehaviour
{
    Vrm10Instance _vrmInstance;
    Coroutine _resetCoroutine;
    string _lastExpr;

    // Supported expressions and their key bindings:
    // 2 = sad, 4 = angry, B = blink, S = surprised, L = blinkLeft, D = lookDown, 0 = reset
    static readonly ExpressionPreset[] _supported = {
        ExpressionPreset.sad,
        ExpressionPreset.angry,
        ExpressionPreset.blink,
        ExpressionPreset.surprised,
        ExpressionPreset.blinkLeft,
        ExpressionPreset.lookDown
    };

    void Start()
    {
        _vrmInstance = GetComponent<Vrm10Instance>();
        if (_vrmInstance == null)
        {
            Debug.LogError("Vrm10Instance component not found!");
        }
    }

    /// <summary>
    /// Sets the given expression for a limited duration, then resets to neutral.
    /// </summary>
    /// <param name="expr">Expression name (see _supported)</param>
    /// <param name="duration">Seconds to hold expression before reset (if zero or negative, defaults to 2s)</param>
    public void SetExpression(string expr, float duration = 2f)
    {
        Debug.Log("[MikaExpressionController] SetExpression called with: " + expr + " duration: " + duration);

        if (_vrmInstance == null || _vrmInstance.Runtime == null)
        {
            Debug.LogWarning("[MikaExpressionController] No VRM instance or runtime found!");
            return;
        }

        if (expr.Equals("reset", System.StringComparison.OrdinalIgnoreCase))
        {
            ResetAll();
            _lastExpr = null;
            return;
        }

        ExpressionPreset presetToSet;
        if (System.Enum.TryParse(expr, true, out presetToSet) && System.Array.Exists(_supported, p => p == presetToSet))
        {
            ResetAll();
            _vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(presetToSet), 1.0f);

            // Use custom duration if positive, else default to 2s
            float useDuration = duration > 0 ? duration : 2f;

            // Start/reset the auto-reset coroutine
            _lastExpr = expr.ToLowerInvariant();
            if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);
            _resetCoroutine = StartCoroutine(ResetAfterDelay(useDuration, _lastExpr));
        }
        else
        {
            Debug.LogWarning("[MikaExpressionController] Unknown or unsupported expression: " + expr);
        }
    }

    /// <summary>
    /// Resets all expression presets to 0.
    /// </summary>
    void ResetAll()
    {
        foreach (ExpressionPreset preset in System.Enum.GetValues(typeof(ExpressionPreset)))
        {
            try
            {
                _vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(preset), 0.0f);
            }
            catch (System.ArgumentException) { continue; }
        }
        Debug.Log("[MikaExpressionController] Reset all expressions.");
    }

    IEnumerator ResetAfterDelay(float delay, string expr)
    {
        yield return new WaitForSeconds(delay);
        // Only reset if expression has not changed during the wait
        if (_lastExpr == expr)
        {
            ResetAll();
            _resetCoroutine = null;
        }
    }

    // For manual testing in the Unity Editor
    void Update()
    {
        if (_vrmInstance == null || _vrmInstance.Runtime == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha2)) SetExpression("sad");
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetExpression("angry");
        if (Input.GetKeyDown(KeyCode.B)) SetExpression("blink");
        if (Input.GetKeyDown(KeyCode.S)) SetExpression("surprised");
        if (Input.GetKeyDown(KeyCode.L)) SetExpression("blinkLeft");
        if (Input.GetKeyDown(KeyCode.D)) SetExpression("lookDown");
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetExpression("reset");
    }
}