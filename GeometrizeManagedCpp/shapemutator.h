#pragma once

#include <cstdint>

namespace geometrize
{
class Circle;
class Ellipse;
class Line;
class Polyline;
class QuadraticBezier;
class Rectangle;
class RotatedEllipse;
class RotatedRectangle;
class Shape;
class Triangle;
}

namespace geometrize
{

// Default implementations that perform initial setup on each type of shape
void setup(geometrize::Shape& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Circle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Ellipse& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Line& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Polyline& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::QuadraticBezier& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Rectangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::RotatedEllipse& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::RotatedRectangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void setup(geometrize::Triangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);

// Default implementations that mutate each type of shape
void mutate(geometrize::Shape& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Circle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Ellipse& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Line& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Polyline& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::QuadraticBezier& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Rectangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::RotatedEllipse& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::RotatedRectangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);
void mutate(geometrize::Triangle& s, std::int32_t xMin, std::int32_t yMin, std::int32_t xMax, std::int32_t yMax);

// Default implementations that translate each type of shape
void translate(geometrize::Shape& s, float x, float y);
void translate(geometrize::Circle& s, float x, float y);
void translate(geometrize::Ellipse& s, float x, float y);
void translate(geometrize::Line& s, float x, float y);
void translate(geometrize::Polyline& s, float x, float y);
void translate(geometrize::QuadraticBezier& s, float x, float y);
void translate(geometrize::Rectangle& s, float x, float y);
void translate(geometrize::RotatedEllipse& s, float x, float y);
void translate(geometrize::RotatedRectangle& s, float x, float y);
void translate(geometrize::Triangle& s, float x, float y);

// Default implementations that scale each type of shape
void scale(geometrize::Shape& s, float scaleFactor);
void scale(geometrize::Circle& s, float scaleFactor);
void scale(geometrize::Ellipse& s, float scaleFactor);
void scale(geometrize::Line& s, float scaleFactor);
void scale(geometrize::Polyline& s, float scaleFactor);
void scale(geometrize::QuadraticBezier& s, float scaleFactor);
void scale(geometrize::Rectangle& s, float scaleFactor);
void scale(geometrize::RotatedEllipse& s, float scaleFactor);
void scale(geometrize::RotatedRectangle& s, float scaleFactor);
void scale(geometrize::Triangle& s, float scaleFactor);

// Default implementations that rotate each type of shape through an angle (those which support rotation anyway)
void rotate(geometrize::Shape& s, float angle);
void rotate(geometrize::Line& s, float angle);
void rotate(geometrize::RotatedEllipse& s, float angle);
void rotate(geometrize::RotatedRectangle& s, float angle);
void rotate(geometrize::Triangle& s, float angle);

}
