using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// ML surrogate model for fast thermodynamic property prediction.
/// Loads pre-trained ONNX models for inference (training done externally in Python).
/// </summary>
public class MLSurrogateModel
{
    private bool _isLoaded = false;

    /// <summary>Whether a model is loaded.</summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Load a pre-trained ONNX model. (Placeholder - requires Microsoft.ML.OnnxRuntime dependency)
    /// </summary>
    public void LoadModel(string onnxPath)
    {
        // ONNX Runtime integration deferred - would require adding NuGet package
        // For now, store path and mark as loaded if file exists
        if (!File.Exists(onnxPath))
            throw new FileNotFoundException($"ONNX model not found: {onnxPath}");
        _isLoaded = true;
    }

    /// <summary>
    /// Predict mineral properties at given P-T using the loaded model.
    /// </summary>
    public TrainingDataPoint Predict(double P, double T)
    {
        if (!_isLoaded)
            throw new InvalidOperationException("No model loaded. Call LoadModel first.");

        // Placeholder for ONNX inference
        throw new NotImplementedException("ONNX inference requires Microsoft.ML.OnnxRuntime package.");
    }
}
