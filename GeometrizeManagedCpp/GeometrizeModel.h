#pragma once

#include "pch.h"
#include "model.h"

public enum class GeometrizeShapeType : int
{
    RECTANGLE = 1U,
    ROTATED_RECTANGLE = 2U,
    TRIANGLE = 4U,
    ELLIPSE = 8U,
    ROTATED_ELLIPSE = 16U,
    CIRCLE = 32U,
    LINE = 64U,
    QUADRATIC_BEZIER = 128U,
    POLYLINE = 256U,
};

public ref class GeometrizeShape
{
public:
    GeometrizeShapeType Type;
    int Color;
    double Score;
};

public ref class GeometrizeShapeCircle : GeometrizeShape {
public:
    GeometrizeShapeCircle();
    GeometrizeShapeCircle(float x, float y, float radius);
    float X;
    float Y;
    float Radius;
};


public ref class GeometrizeModel
{
private:
    geometrize::Model* model;
    geometrize::Bitmap* bitmap;

public:
    GeometrizeModel(int width, int height, array<System::Byte>^ imageData);
    ~GeometrizeModel();

    array<GeometrizeShape^>^ Step(
        GeometrizeShapeType shapeTypes,
        System::Byte alpha,
        System::UInt32 shapeCount,
        System::UInt32 shapeMutations,
        System::UInt32 maxThreads);
};