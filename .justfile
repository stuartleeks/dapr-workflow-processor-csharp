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

run-workflow2-no-retries:
	dapr run --run-file dapr-workflow2-no-retries.yaml

stop-workflow2-no-retries:
	dapr stop --run-file dapr-workflow2-no-retries.yaml

############################################################################
# recipes for submitting and watching jobs

submit-job-simple:
	echo '{"steps": [{"name": "simple_test","actions" : [{"name": "simple_action", "action": "processor1","content" : "Hello World"}]}]}'  \
	| ./scripts/run-workflow.sh

submit-job-multi-step:
	echo '{"steps": [{"name": "parallel_step","actions" : [{"name": "action1.1", "action": "processor1","content" : "Hello World"},{"name": "action1.2", "action": "processor1","content" : "Do stuff"},{"name": "action1.3", "action": "processor1","content" : "Do more stuff"}]},{"name": "final_step","actions" : [{"name": "action2.1", "action": "processor1","content" : "Finale"}]}]}'  \
	| ./scripts/run-workflow.sh

submit-job-multi-step-with-processor2:
	echo '{"steps": [{"name": "parallel_step","actions" : [{"name": "action1.1", "action": "processor2","content" : "Hello World"},{"name": "action1.2", "action": "processor1","content" : "Do stuff"},{"name": "action1.3", "action": "processor1","content" : "Do more stuff"}]},{"name": "final_step","actions" : [{"name": "action2.1", "action": "processor1","content" : "Finale"}]}]}'  \
	| ./scripts/run-workflow.sh