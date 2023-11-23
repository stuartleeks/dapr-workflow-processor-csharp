default:
	just --list

############################################################################
# recipes for running multiple services in different configurations

run-workflow1-no-retries:
	dapr run --run-file dapr-workflow1-no-retries.yaml

	
stop-workflow1-no-retries:
	dapr stop --run-file dapr-workflow1-no-retries.yaml

run-workflow1-dapr-retries:
	dapr run --run-file dapr-workflow1-dapr-retries.yaml
	
stop-workflow1-dapr-retries:
	dapr stop --run-file dapr-workflow1-dapr-retries.yaml

############################################################################
# recipes for submitting and watching jobs

submit-job-simple:
	echo '{"steps": [{"name": "simple_test","actions" : [{"action": "processor1","content" : "Hello World"}]}]}'  \
	| ./scripts/run-workflow.sh

submit-job-multi-step:
	echo '{"steps": [{"name": "parallel_step","actions" : [{"action": "processor1","content" : "Hello World"},{"action": "processor1","content" : "Do stuff"},{"action": "processor1","content" : "Do more stuff"}]},{"name": "final_step","actions" : [{"action": "processor1","content" : "Finale"}]}]}'  \
	| ./scripts/run-workflow.sh

submit-job-multi-step-with-processor2:
	echo '{"steps": [{"name": "parallel_step","actions" : [{"action": "processor2","content" : "Hello World"},{"action": "processor1","content" : "Do stuff"},{"action": "processor1","content" : "Do more stuff"}]},{"name": "final_step","actions" : [{"action": "processor1","content" : "Finale"}]}]}'  \
	| ./scripts/run-workflow.sh