#include "pch.h"
#include "GeometrizeModel.h"
#include "bitmap.h"
#include "shapefactory.h"
#include "circle.h"

using namespace geometrize;

GeometrizeModel::GeometrizeModel(int width, int height, cli::array<System::Byte>^ imageData)
{
	pin_ptr<System::Byte> p = &imageData[0];
	unsigned char* uchars = p;
	char* chars = reinterpret_cast<char*>(uchars);
	int sz = imageData->Length;
	std::vector<std::uint8_t> items = std::vector<std::uint8_t>(sz);
	for (int n = sz - 1; n >= 0; n--) {
		items[n] = (unsigned int)chars[n];
	}

	this->bitmap = new Bitmap(width, height, items);
	
	this->model = new Model(*this->bitmap);
}

GeometrizeModel::~GeometrizeModel() {
	delete this->model;
	delete this->bitmap;
}

array<GeometrizeShape^>^ GeometrizeModel::Step(
	GeometrizeShapeType types,
	System::Byte alpha,
	System::UInt32 shapeCount,
	System::UInt32 shapeMutations,
	System::UInt32 maxThreads)
{
	std::function<std::shared_ptr<Shape>()> shapeCreator =
		createDefaultShapeCreator(static_cast<ShapeTypes>(types), 0, 0, this->bitmap->getWidth(), this->bitmap->getHeight());

	std::vector<ShapeResult> results = 
		this->model->step(shapeCreator, alpha, shapeCount, shapeMutations, maxThreads);

	array<GeometrizeShape^>^ output = gcnew array<GeometrizeShape^>(results.size());

	unsigned int vecSize = results.size();

	for (unsigned int i = 0; i < vecSize; i++)
	{
		ShapeTypes type = results[i].shape->getType();

		switch (type) {
		case ShapeTypes::CIRCLE:
			Circle* circle = dynamic_cast<Circle*>(results[i].shape.get());
			output[i] = gcnew GeometrizeShapeCircle(circle->m_x, circle->m_y, circle->m_r);
			break;
		}

		output[i]->Score = results[i].score;
		output[i]->Color = (results[i].color.r << 24) + (results[i].color.g << 16) + (results[i].color.b << 8) + results[i].color.a;
	}

	return output;
}

GeometrizeShapeCircle::GeometrizeShapeCircle()
{
}

GeometrizeShapeCircle::GeometrizeShapeCircle(float x, float y, float radius)
{
	this->X = x;
	this->Y = y;
	this->Radius = radius;
}