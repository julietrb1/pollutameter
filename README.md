# Pollutameter

Air quality measurement, without the sensors.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=julietrb1_pollutameter&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=julietrb1_pollutameter)
[![Dotnet Build Status](https://github.com/julietrb1/pollutameter/actions/workflows/dotnet.yml/badge.svg)](https://github.com/julietrb1/pollutameter/actions/workflows/dotnet.yml/badge.svg)

## What Do?

This casual .NET side-project implements a proximity-based approximation of air quality calculation (currently only for 
New South Wales, Australia). Given your latitude and longitude, it will find the nearest sensor readings using the government's
[Air Quality Data API](https://data.airquality.nsw.gov.au/docs/index.html).

It combines these readings by calculating their distance from your position, weighting readings
from sensors nearest to you using the [Haversine formula](http://en.wikipedia.org/wiki/Haversine_formula). With this,
the inverse distance provides a weight, and that all gets mashed together into a final number.

## License

Licensed as copyleft under the [GNU General Public License v3.0](https://spdx.org/licenses/GPL-3.0-or-later.html).
Because sharing is caring! :)
