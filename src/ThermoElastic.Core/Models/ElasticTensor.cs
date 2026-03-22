namespace ThermoElastic.Core.Models;

/// <summary>
/// 6x6 Voigt notation elastic stiffness tensor [GPa].
/// </summary>
public class ElasticTensor
{
    /// <summary>Cij matrix in Voigt notation (6x6) [GPa]</summary>
    public double[,] C { get; set; } = new double[6, 6];

    /// <summary>Density [g/cm³]</summary>
    public double Density { get; set; }

    /// <summary>Mineral name</summary>
    public string MineralName { get; set; } = string.Empty;

    /// <summary>Isotropic Voigt bulk modulus [GPa]</summary>
    public double KVoigt => (C[0,0] + C[1,1] + C[2,2] + 2*(C[0,1] + C[0,2] + C[1,2])) / 9.0;

    /// <summary>Isotropic Voigt shear modulus [GPa]</summary>
    public double GVoigt => ((C[0,0] + C[1,1] + C[2,2]) - (C[0,1] + C[0,2] + C[1,2]) + 3*(C[3,3] + C[4,4] + C[5,5])) / 15.0;

    /// <summary>Maximum P-wave anisotropy [%]</summary>
    public double MaxAnisotropy { get; set; }
}
