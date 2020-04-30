# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [5.3.0] - 2019-11-20
### Changed
 - Using new GenericBaseUtils to standardize crestron device setup and teardown

## [5.2.4] - 2020-04-30
### Added
 - Fusion: Telemetry provider whitelist to Fusion Sig Mappings
 - MockFusionRoom with assets, for testing

### Changed
 - Fusion: A null value sends an empty string
 - Fusion: Removed Audio and USB Breakaway mapping sigs
 - Fusion: Refactored Sig Binding
 - Fusion: Munger - Removed unused Type for mappings
 - Fusion: Updated Mappings to move comonly used attributes to Standard Sig Mappings

## [5.2.3] - 2019-12-03
### Added
 - Added error catching around adding sigs and assets to fusion to better log what sigs are causing errors.

## [5.2.2] - 2019-08-01
### Changed
 - Substantial performance improvements in telemetry instantiation

## [5.2.1] - 2019-06-14
### Changed
 - Using new RangeAttribute methods for safer remapping of ushort to/from numeric types
 - Failing gracefully when a numeric value fails to remap

## [5.2.0] - 2019-06-06
### Added
 - Exposed features for generating Fusion RVI files

### Changed
 - Fixed bug where Fusion telemetry was not being generated for Panels
 - FusionTelemetryMunger is no longer generating RVI on completion

## [5.1.0] - 2019-05-21
### Added
 - FusionTelemetryMunger supports registering multiple mapping sets under existing mapping types
 - Added util method for converting a log item into fusion log text format

## [5.0.0] - 2019-05-16
### Added
 - Added attributes for decorating telemetry properties, methods and events
 - Added telemetry nodes and collections for describing telemetry features as a hierarchy
 - Added Fusion static asset support to interfaces and abstractions
 - Added mappings and bindings for feeding telemetry data to/from Fusion
 - Added Fusion Room sig support

## [4.0.0] - 2018-10-18
### Changed
 - Analytics renamed to Telemetry
 - FusionPro renamed to CrestronPro

## [3.0.3] - 2018-09-14
### Changed
 - Fixing using directives

## [3.0.2] - 2018-07-02
### Changed
 - Sig collections are now public

## [3.0.1] - 2018-05-09
### Changed
 - Fixing issues with loading UTF8-BOM configs

## [3.0.0] - 2018-04-24
### Changed
 - Fusion RemoteOccupancySensor asset uses enum for occupancy state instead of bool. Now supports unknown state.
 - Removing suffix from assembly names
