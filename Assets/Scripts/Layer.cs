using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class Layer
{
	Matrix<float> weights;

	Func<float, float> activation;

	public int Inputs { get { return weights.RowCount; } }
	public int Outputs { get { return weights.ColumnCount; } }

	public Layer(int inputNeurons, int outputNeurons, Func<float, float> activationFunction)
	{
		weights = Matrix<float>.Build.Random(inputNeurons, outputNeurons);

		activation = new Func<float, float>(activationFunction);
	}

	public Layer(Layer other)
	{
		weights = Matrix<float>.Build.DenseOfMatrix(other.weights);
		activation = other.activation;
	}

	public Vector<float> ProcessLayer(Vector<float> vector)
	{
		var returnVal = vector * weights;
		returnVal.Map(activation, returnVal, Zeros.Include);

		return returnVal;
	}

	public float[] ProcessLayer(float[] array)
	{
		return ProcessLayer(Vector<float>.Build.DenseOfArray(array))
			.ToArray();
	}

	public void Mutate(float magnitude)
	{
		weights = weights.Map((x) => UnityEngine.Random.Range(-magnitude, magnitude) + x, Zeros.Include);
	}

	public float Sum()
	{
		return weights.ColumnSums().Sum();
	}
}
