from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path

import numpy as np
import rasterio
from rasterio.enums import Resampling
from rasterio.transform import from_origin
from rasterio.warp import reproject


SCHEMA_VERSION = "ssalddel-spatial-layer-artifacts.v1"
RULE_REVISION = "daegwallyeong-l2-physical-spatial.r1"
DEFAULT_TILE_KEY = "kr5186:l2:700:1145"
TILE_SIZE_METERS = 500
HALO_METERS = 60
SAMPLE_SPACING_METERS = 10


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def parse_tile_key(value: str) -> tuple[int, int]:
    parts = value.split(":")
    if len(parts) != 4 or parts[0] != "kr5186" or parts[1] != "l2":
        raise ValueError(f"TileKeyInvalid:{value}")
    return int(parts[2]), int(parts[3])


def reproject_height_vertices(
    source_path: Path,
    min_easting: float,
    min_northing: float,
    max_easting: float,
    max_northing: float,
) -> np.ndarray:
    width = int(round((max_easting - min_easting) / SAMPLE_SPACING_METERS)) + 1
    height = int(round((max_northing - min_northing) / SAMPLE_SPACING_METERS)) + 1
    destination = np.full((height, width), np.nan, dtype="float32")
    transform = from_origin(
        min_easting - SAMPLE_SPACING_METERS / 2,
        max_northing + SAMPLE_SPACING_METERS / 2,
        SAMPLE_SPACING_METERS,
        SAMPLE_SPACING_METERS,
    )
    with rasterio.open(source_path) as source:
        if str(source.crs) != "EPSG:5186":
            raise ValueError(f"DemCrsInvalid:{source.crs}")
        reproject(
            source=rasterio.band(source, 1),
            destination=destination,
            src_transform=source.transform,
            src_crs=source.crs,
            src_nodata=source.nodata,
            dst_transform=transform,
            dst_crs="EPSG:5186",
            dst_nodata=np.nan,
            resampling=Resampling.bilinear,
        )
    return destination


def reproject_land_cover_cells(
    source_path: Path,
    min_easting: float,
    min_northing: float,
    max_easting: float,
    max_northing: float,
) -> np.ndarray:
    width = int(round((max_easting - min_easting) / SAMPLE_SPACING_METERS))
    height = int(round((max_northing - min_northing) / SAMPLE_SPACING_METERS))
    destination = np.zeros((height, width), dtype="uint8")
    transform = from_origin(
        min_easting,
        max_northing,
        SAMPLE_SPACING_METERS,
        SAMPLE_SPACING_METERS,
    )
    with rasterio.open(source_path) as source:
        if str(source.crs) != "EPSG:5186":
            raise ValueError(f"LandCoverCrsInvalid:{source.crs}")
        reproject(
            source=rasterio.band(source, 1),
            destination=destination,
            src_transform=source.transform,
            src_crs=source.crs,
            src_nodata=source.nodata,
            dst_transform=transform,
            dst_crs="EPSG:5186",
            dst_nodata=0,
            resampling=Resampling.nearest,
        )
    return destination


def build_placement_mask(height_vertices: np.ndarray, land_cover: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    top_left = height_vertices[:-1, :-1]
    top_right = height_vertices[:-1, 1:]
    bottom_left = height_vertices[1:, :-1]
    bottom_right = height_vertices[1:, 1:]
    valid_height = (
        np.isfinite(top_left)
        & np.isfinite(top_right)
        & np.isfinite(bottom_left)
        & np.isfinite(bottom_right)
    )
    dz_dx = ((top_right + bottom_right) - (top_left + bottom_left)) / (2 * SAMPLE_SPACING_METERS)
    dz_dz = ((bottom_left + bottom_right) - (top_left + top_right)) / (2 * SAMPLE_SPACING_METERS)
    slope_degrees = np.degrees(np.arctan(np.hypot(dz_dx, dz_dz))).astype("float32")
    slope_degrees[~valid_height] = np.nan

    valid = valid_height & (land_cover != 0)
    water = np.isin(land_cover, np.array([80, 90], dtype="uint8"))
    mask = np.zeros(land_cover.shape, dtype="uint8")
    mask[valid] |= np.uint8(1 << 0)
    mask[water] |= np.uint8(1 << 1)
    mask[np.isfinite(slope_degrees) & (slope_degrees > 12.0)] |= np.uint8(1 << 2)
    mask[np.isfinite(slope_degrees) & (slope_degrees > 20.0)] |= np.uint8(1 << 3)
    mask[land_cover == 50] |= np.uint8(1 << 4)
    mask[land_cover == 40] |= np.uint8(1 << 5)
    mask[land_cover == 10] |= np.uint8(1 << 6)
    mask[~valid] |= np.uint8(1 << 7)
    return mask, slope_degrees


def artifact_descriptor(path: Path, root: Path, format_code: str, width: int, height: int) -> dict[str, object]:
    return {
        "formatCode": format_code,
        "relativePath": path.relative_to(root).as_posix(),
        "sha256": sha256_file(path),
        "byteLength": path.stat().st_size,
        "width": width,
        "height": height,
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--tile-key", default=DEFAULT_TILE_KEY)
    parser.add_argument(
        "--dem",
        type=Path,
        default=Path("artifacts/local/public-spatial/pyeongchang/pyeongchang-copernicus-dem-glo30-epsg5186.tif"),
    )
    parser.add_argument(
        "--land-cover",
        type=Path,
        default=Path("artifacts/local/public-spatial/pyeongchang/pyeongchang-esa-worldcover-2021-v200-epsg5186.tif"),
    )
    parser.add_argument(
        "--artifact-root",
        type=Path,
        default=Path("artifacts/local/public-spatial/pyeongchang"),
    )
    parser.add_argument(
        "--manifest",
        type=Path,
        default=Path("eng/public-spatial/manifests/kr5186-l2-700-1145.json"),
    )
    args = parser.parse_args()

    tile_x, tile_y = parse_tile_key(args.tile_key)
    core_min_easting = tile_x * TILE_SIZE_METERS
    core_min_northing = tile_y * TILE_SIZE_METERS
    core_max_easting = core_min_easting + TILE_SIZE_METERS
    core_max_northing = core_min_northing + TILE_SIZE_METERS
    generation_min_easting = core_min_easting - HALO_METERS
    generation_min_northing = core_min_northing - HALO_METERS
    generation_max_easting = core_max_easting + HALO_METERS
    generation_max_northing = core_max_northing + HALO_METERS

    dem_path = args.dem.resolve()
    land_cover_path = args.land_cover.resolve()
    artifact_root = args.artifact_root.resolve()
    if not dem_path.exists():
        raise FileNotFoundError(f"DemSourceMissing:{dem_path}")
    if not land_cover_path.exists():
        raise FileNotFoundError(f"LandCoverSourceMissing:{land_cover_path}")

    tile_directory = artifact_root / "generated" / "tiles" / args.tile_key.replace(":", "_")
    tile_directory.mkdir(parents=True, exist_ok=True)
    height_path = tile_directory / "elevation.height-f32-v1.bin"
    land_cover_output_path = tile_directory / "land-cover.landcover-u8-v1.bin"
    placement_mask_path = tile_directory / "placement.placement-mask-u8-v1.bin"

    height_vertices = reproject_height_vertices(
        dem_path,
        generation_min_easting,
        generation_min_northing,
        generation_max_easting,
        generation_max_northing,
    )
    land_cover = reproject_land_cover_cells(
        land_cover_path,
        generation_min_easting,
        generation_min_northing,
        generation_max_easting,
        generation_max_northing,
    )
    placement_mask, slope_degrees = build_placement_mask(height_vertices, land_cover)

    height_vertices.astype("<f4", copy=False).tofile(height_path)
    land_cover.astype("uint8", copy=False).tofile(land_cover_output_path)
    placement_mask.astype("uint8", copy=False).tofile(placement_mask_path)

    finite_heights = height_vertices[np.isfinite(height_vertices)]
    finite_slopes = slope_degrees[np.isfinite(slope_degrees)]
    if finite_heights.size == 0:
        raise ValueError("ElevationArtifactContainsNoValidSamples")

    source_hashes = {
        "dem": sha256_file(dem_path),
        "landCover": sha256_file(land_cover_path),
    }
    fingerprint_payload = "|".join(
        [args.tile_key, RULE_REVISION, source_hashes["dem"], source_hashes["landCover"]]
    )
    fingerprint = hashlib.sha256(fingerprint_payload.encode("utf-8")).hexdigest().upper()

    manifest = {
        "schemaVersion": SCHEMA_VERSION,
        "ruleRevision": RULE_REVISION,
        "tileKey": args.tile_key,
        "coordinateReferenceSystem": "EPSG:5186",
        "tileSizeMeters": TILE_SIZE_METERS,
        "haloMeters": HALO_METERS,
        "sampleSpacingMeters": SAMPLE_SPACING_METERS,
        "coreBounds": {
            "minEasting": core_min_easting,
            "minNorthing": core_min_northing,
            "maxEasting": core_max_easting,
            "maxNorthing": core_max_northing,
        },
        "generationBounds": {
            "minEasting": generation_min_easting,
            "minNorthing": generation_min_northing,
            "maxEasting": generation_max_easting,
            "maxNorthing": generation_max_northing,
        },
        "sources": {
            "elevation": {
                "providerCode": "CopernicusDEM",
                "sourceRevision": "Copernicus-DEM-GLO30-N37E128",
                "sourceReferenceDate": None,
                "horizontalCrs": "EPSG:5186",
                "verticalDatum": "Unverified",
                "physicalElevationUnit": "meter",
                "resolutionMeters": 30,
                "noDataValue": -32767,
                "sha256": source_hashes["dem"],
            },
            "landCover": {
                "providerCode": "ESAWorldCover",
                "sourceRevision": "ESA-WorldCover-2021-v200-N36E126",
                "sourceReferenceDate": "2021",
                "horizontalCrs": "EPSG:5186",
                "resolutionMeters": 10,
                "noDataValue": 0,
                "sha256": source_hashes["landCover"],
            },
        },
        "artifacts": {
            "elevation": artifact_descriptor(
                height_path,
                artifact_root,
                "height-f32-v1",
                height_vertices.shape[1],
                height_vertices.shape[0],
            ),
            "landCover": artifact_descriptor(
                land_cover_output_path,
                artifact_root,
                "landcover-u8-v1",
                land_cover.shape[1],
                land_cover.shape[0],
            ),
            "placementMask": artifact_descriptor(
                placement_mask_path,
                artifact_root,
                "placement-mask-u8-v1",
                placement_mask.shape[1],
                placement_mask.shape[0],
            ),
        },
        "placementMaskBits": {
            "0": "ValidTerrain",
            "1": "Water",
            "2": "SlopeAbove12Degrees",
            "3": "SlopeAbove20Degrees",
            "4": "BuiltArea",
            "5": "AgricultureCandidate",
            "6": "ForestCandidate",
            "7": "NoData",
        },
        "statistics": {
            "validElevationSampleCount": int(finite_heights.size),
            "minimumPhysicalElevationMeters": round(float(finite_heights.min()), 4),
            "maximumPhysicalElevationMeters": round(float(finite_heights.max()), 4),
            "maximumSlopeDegrees": round(float(finite_slopes.max()), 4) if finite_slopes.size else None,
            "waterCellCount": int(np.count_nonzero(placement_mask & (1 << 1))),
            "agricultureCandidateCellCount": int(np.count_nonzero(placement_mask & (1 << 5))),
            "forestCandidateCellCount": int(np.count_nonzero(placement_mask & (1 << 6))),
            "noDataCellCount": int(np.count_nonzero(placement_mask & (1 << 7))),
        },
        "fingerprintSha256": fingerprint,
        "physicalElevationIsAuthoritativeForPlacement": True,
        "visualHeightExaggerationStoredSeparately": True,
        "isOperationalState": False,
    }

    args.manifest.parent.mkdir(parents=True, exist_ok=True)
    args.manifest.write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    print(f"SpatialTileArtifactsCreated:{args.tile_key}")
    print(f"Manifest:{args.manifest.as_posix()}")
    print(f"Fingerprint:{fingerprint}")


if __name__ == "__main__":
    main()
