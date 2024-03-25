package solutions

import (
	"bufio"
	"errors"
	"fmt"
	"io"
	"os"
	"slices"
	"strconv"
	"strings"
)

func V1Simplest(inputPath string) (err error) {
	dict := make(map[string][]float64)
	file, err := os.Open(inputPath)
	if err != nil {
		return err
	}
	defer func() {
		closeErr := file.Close()
		if err == nil {
			err = closeErr
		} else {
			err = errors.Join(err, closeErr)
		}
	}()

	reader := bufio.NewReader(file)

	for {
		line, err := reader.ReadString('\n')
		if err == io.EOF {
			break
		} else if err != nil {
			return err
		}
		line = line[:len(line)-1] // remove trailing \n
		values := strings.Split(line, ";")
		name := values[0]
		measurement, err := strconv.ParseFloat(values[1], 64)
		if err != nil {
			return err
		}
		measurements, exists := dict[name]
		if exists {
			dict[name] = append(measurements, measurement)
		} else {
			dict[name] = []float64{measurement}
		}
	}

	results := make([]Result, 0, len(dict))
	for stationName, measurements := range dict {
		results = append(results, Result{stationName, measurements})
	}
	slices.SortFunc(results, func(a Result, b Result) int {
		return strings.Compare(a.stationName, b.stationName)
	})

	fmt.Print("{")
	for _, r := range results {
		sum := float64(0)
		for _, m := range r.measurements {
			sum += m
		}
		mean := sum / float64(len(r.measurements))
		fmt.Printf("%s=%f/%f/%f, ", r.stationName, slices.Min(r.measurements), mean, slices.Max(r.measurements))
	}
	fmt.Print("}")

	return nil
}

type Result struct {
	stationName  string
	measurements []float64
}
