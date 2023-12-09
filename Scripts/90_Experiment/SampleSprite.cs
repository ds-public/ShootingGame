using Godot;
using System;

/// <summary>
/// テスト
/// </summary>
[Tool]
public partial class SampleSprite : Sprite2D
{
	public override void _Ready()
	{
	}

	public override void _Process( double delta )
	{
		Rotation += Mathf.Pi * ( float )delta ;
	}
}
