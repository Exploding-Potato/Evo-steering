using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNet
{
	private Layer[] layers;

	public NeuralNet (int[] layerSizes, Func<float, float> activationFunction)
	{
		// "First" layer isn't a Layer object
		// So layers has only layerSizes.Count-1 elements
		layers = new Layer[layerSizes.Length - 1];
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i] = new Layer(
				// To account for the bias unit
				layerSizes[i] + 1,
				layerSizes[i + 1],
				activationFunction);
		}
	}

	public NeuralNet (NeuralNet other)
	{
		layers = new Layer[other.layers.Length];
		for (int i = 0; i < layers.Length; i++)
			layers[i] = new Layer(other.layers[i]);
	}

	public float Sum()
	{
		float sum = 0;
		foreach (Layer layer in layers)
			sum += layer.Sum();
		return sum;
	}

	public Vector<float> Predict(Vector<float> input)
	{
		foreach (var layer in layers)
		{
			// Network automatically appends the bias unit
			input = Vector<float>.Build.DenseOfEnumerable(input.Append(1));
			input = layer.ProcessLayer(input);
		}

		return input;
	}

	public float[] Predict(float[] input)
	{
		return Predict(Vector<float>.Build.DenseOfArray(input))
			.ToArray();
	}

	public void Mutate(float magnitude)
	{
		foreach (Layer layer in layers)
			layer.Mutate(magnitude);
	}
}