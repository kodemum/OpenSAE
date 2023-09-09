#pragma once

#include <cstdint>
#include <functional>
#include <memory>
#include <vector>

#include "rgba.h"
#include "scanline.h"
#include "state.h"

namespace geometrize
{
class Bitmap;
}

namespace geometrize
{

namespace core
{

/**
 * The core functions for Geometrize.
 * @author Sam Twidale (https://samcodes.co.uk/)
 */

/**
 * @brief EnergyFunction Type alias for a function that calculates a measure of the improvement adding the scanlines of a shape provides - lower energy is better.
 * @param lines The scanlines of the shape.
 * @param alpha The alpha of the scanlines.
 * @param target The target bitmap.
 * @param current The current bitmap.
 * @param buffer The buffer bitmap.
 * @param score The score.
 * @return The energy measure.
 */
using EnergyFunction = std::function<double(
    const std::vector<geometrize::Scanline>& lines,
    const std::uint32_t alpha,
    const geometrize::Bitmap& target,
    const geometrize::Bitmap& current,
    geometrize::Bitmap& buffer,
    double score)>;

/**
 * @brief defaultEnergyFunction The default/built-in energy function that calculates a measure of the improvement adding the scanlines of a shape provides - lower energy is better.
 * @param lines The scanlines of the shape.
 * @param alpha The alpha of the scanlines.
 * @param target The target bitmap.
 * @param current The current bitmap.
 * @param buffer The buffer bitmap.
 * @param score The score.
 * @return The energy measure.
 */
double defaultEnergyFunction(
        const std::vector<geometrize::Scanline>& lines,
        const std::uint32_t alpha,
        const geometrize::Bitmap& target,
        const geometrize::Bitmap& current,
        geometrize::Bitmap& buffer,
        double score);

/**
 * @brief computeColor Calculates the color of the scanlines.
 * @param target The target image.
 * @param current The current image.
 * @param lines The scanlines.
 * @param alpha The alpha of the scanline.
 * @return The color of the scanlines.
 */
geometrize::rgba computeColor(
        const geometrize::Bitmap& target,
        const geometrize::Bitmap& current,
        const std::vector<geometrize::Scanline>& lines,
        std::uint8_t alpha);

/**
 * @brief differenceFull Calculates the root-mean-square error between two bitmaps.
 * @param first The first bitmap.
 * @param second The second bitmap.
 * @return The difference/error measure between the two bitmaps.
 */
double differenceFull(const geometrize::Bitmap& first, const geometrize::Bitmap& second);

/**
 * @brief differencePartial Calculates the root-mean-square error between the parts of the two bitmaps within the scanline mask.
 * This is for optimization purposes, it lets us calculate new error values only for parts of the image we know have changed.
 * @param target The target bitmap.
 * @param before The bitmap before the change.
 * @param after The bitmap after the change.
 * @param score The score.
 * @param lines The scanlines.
 * @return The difference/error between the two bitmaps, masked by the scanlines.
 */
double differencePartial(
        const geometrize::Bitmap& target,
        const geometrize::Bitmap& before,
        const geometrize::Bitmap& after,
        double score,
        const std::vector<Scanline>& lines);

/**
 * @brief bestHillClimbState Gets the best state using a hill climbing algorithm.
 * @param shapeCreator A function that will create the shapes that will be chosen from.
 * @param alpha The opacity of the shape.
 * @param n The number of random states to generate.
 * @param age The number of hillclimbing steps.
 * @param target The target bitmap.
 * @param current The current bitmap.
 * @param buffer The buffer bitmap.
 * @param lastScore The last score.
 * @param customEnergyFunction An optional function to calculate the energy (if unspecified a default implementation is used).
 * @return The best state acquired from hill climbing i.e. the one with the lowest energy.
 */
geometrize::State bestHillClimbState(
        const std::function<std::shared_ptr<geometrize::Shape>(void)>& shapeCreator,
        std::uint32_t alpha,
        std::uint32_t n,
        std::uint32_t age,
        const geometrize::Bitmap& target,
        const geometrize::Bitmap& current,
        geometrize::Bitmap& buffer,
        double lastScore,
        const EnergyFunction& customEnergyFunction = nullptr);

}

}
