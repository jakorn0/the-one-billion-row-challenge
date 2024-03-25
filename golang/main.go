package main

import (
	"fmt"
	"golang/solutions"
	"time"
)

func main() {
	startTime := time.Now()
	inputPath := ""
	err := solutions.V1Simplest(inputPath)
	if err != nil {
		panic(err)
	}
	elapsed := time.Since(startTime)
	fmt.Printf("\n\n%s", elapsed)
}
