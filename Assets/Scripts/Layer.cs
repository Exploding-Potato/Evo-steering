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

	public Vector<float> ProcessArray(Vector<float> vector)
	{
		var returnVal = vector * weights;
		returnVal.Map(activation, returnVal, Zeros.Include);

		return returnVal;
	}

	public float[] ProcessArray(float[] array)
	{
		return ProcessArray(Vector<float>.Build.DenseOfArray(array))
			.ToArray();
	}

	public static Layer Mutate(Layer layer, float magnitude)
	{
		Layer returnVal = new Layer(layer);
		returnVal.weights = returnVal.weights.Map((x) => UnityEngine.Random.Range(-magnitude, magnitude) + x, Zeros.Include);

		return returnVal;
	}

	public static Layer Recombnine(Layer layer1, Layer layer2)
	{
		layer1.weights += layer2.weights;

		return layer1;
	}

	public float Sum()
	{
		return weights.ColumnSums().Sum();
	}
}
