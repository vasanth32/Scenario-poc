# Docker and Kubernetes Learning Notes

## What We Built

This project is an ASP.NET Core Payment API. We packaged it as a Docker image, ran it locally, and deployed two replicas through Kubernetes.

Important files:

- `Dockerfile`: instructions Docker uses to build the application image.
- `.dockerignore`: prevents local build files from being copied into the image.
- `k8s/pod.yaml`: a simple standalone-pod example.
- `k8s/deployment.yaml`: manages two Payment API pod replicas.
- `k8s/service.yaml`: provides a stable network endpoint for the replicas.

## Docker Basics

### Image and container

- An **image** is a packaged application template. It contains the application, its runtime, and required files.
- A **container** is a running instance of an image.

Think of an image as a class and a container as an object created from that class.

### Our Dockerfile

The Dockerfile uses a multi-stage build:

1. The `sdk:8.0` image restores packages and publishes the .NET application.
2. The smaller `aspnet:8.0` image runs the published application.

The API listens on port `8080` **inside the container**:

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
```

`EXPOSE` documents the container port. It does not publish the application to your computer by itself.

### Build the image

Run this from the `Payment.Api` folder:

```powershell
docker build -t payment-api:day1 .
```

What it means:

- `docker build`: builds an image from the Dockerfile.
- `-t payment-api:day1`: names the image and gives it the `day1` tag.
- `.`: uses the current folder as the build context.

To see local images:

```powershell
docker images
```

### Run locally with Docker

```powershell
docker run --rm -p 5000:8080 --name payment-api payment-api:day1
```

- `5000`: port on your Windows computer.
- `8080`: port inside the container.
- `--rm`: removes the container when it stops.
- `--name`: gives the container a friendly name.

Open `http://localhost:5000` while the container is running.

We first tried ports `8080` and `8081`, but Windows had reserved them. Port `5000` worked. This did **not** require changing the application or pod port; only the host-side Docker port changed.

Useful Docker commands:

```powershell
docker ps
docker logs payment-api
docker stop payment-api
docker image ls
```

## Why the First Docker Build Failed

The build failed at `dotnet restore` because the local .NET SDK Docker image contained a corrupted, empty runtime configuration file. The application code and Dockerfile were valid.

We fixed it by removing and downloading the SDK image again:

```powershell
docker image rm mcr.microsoft.com/dotnet/sdk:8.0
docker pull mcr.microsoft.com/dotnet/sdk:8.0
```

We also added `.dockerignore`:

```text
bin/
obj/
```

These folders contain local generated .NET build files. Docker should not copy them into the build context.

## Kubernetes Basics

### Cluster, node, pod, and container

- A **cluster** is the Kubernetes environment that manages applications.
- A **node** is a machine that runs workloads in the cluster.
- A **pod** is Kubernetes' smallest deployable unit.
- A pod usually contains one or more **containers**.

Our `payment-api` Deployment creates pods that each contain one container named `payment-api`.

### Pod manifest and Deployment

`k8s/pod.yaml` was useful for learning the smallest Kubernetes unit. In real applications, use a Deployment instead. A Deployment manages pods for you: it creates the desired number of replicas, replaces failed pods, and supports rolling updates.

The current `k8s/deployment.yaml` has:

```yaml
kind: Deployment
replicas: 2
image: payment-api:day1
containerPort: 8080
```

The image tag must match the image you build. `payment-api:latest` and `payment-api:day1` are different tags, even when they may point to the same application version.

The Deployment selector and pod-template labels must match. In this project they both use:

```yaml
app: payment-api
```

Kubernetes uses this label to know which pods belong to the Deployment and which pods receive traffic from the Service.

### Service

The `payment-api-service` Service selects pods with `app: payment-api` and sends incoming traffic from port `80` to container port `8080`:

```yaml
type: NodePort
port: 80
targetPort: 8080
```

- `port: 80`: the Service port inside the cluster.
- `targetPort: 8080`: the port where the API listens inside each pod.
- `NodePort`: exposes the Service through a port on the Kubernetes node. The current assigned node port is `31199`.

A Service gives callers one stable address while Kubernetes can add, remove, or replace the individual pods behind it.

### Health probes: important correction

`readinessProbe` and `livenessProbe` must be under the container, at the same indentation level as `image` and `ports`. In the current `deployment.yaml`, they are outside `containers`, so they are not configured in the live Deployment.

Use this structure when you update the manifest:

```yaml
containers:
	- name: payment-api
		image: payment-api:day1
		ports:
			- containerPort: 8080
		readinessProbe:
			httpGet:
				path: /health
				port: 8080
			initialDelaySeconds: 5
			periodSeconds: 10
		livenessProbe:
			httpGet:
				path: /health
				port: 8080
			initialDelaySeconds: 10
			periodSeconds: 15
```

- A **readiness probe** decides whether a healthy-enough pod can receive Service traffic.
- A **liveness probe** detects a stuck or unhealthy application; Kubernetes restarts its container when it fails.
- The API must implement the `/health` endpoint before these probes can pass.

### Create and check the Deployment

```powershell
kubectl apply -f .\k8s\deployment.yaml
kubectl apply -f .\k8s\service.yaml
kubectl get deployments
kubectl get pods
kubectl get services
```

A healthy result looks like this:

```text
payment-api-...   1/1   Running   0   ...
```

- `1/1`: one container is ready out of one expected container.
- `Running`: Kubernetes started the pod successfully.
- `0` restarts: the container has not crashed and restarted.

More useful Kubernetes commands:

```powershell
kubectl describe deployment payment-api
kubectl get pods -l app=payment-api
kubectl logs <pod-name>
kubectl describe pod <pod-name>
kubectl get service payment-api-service
```

## Why Kubernetes Showed ErrImagePull

The pod manifest asked for `payment-api:day1`, but only `payment-api:latest` had been built. Kubernetes could not find `day1`, so it attempted to pull it and showed `ErrImagePull` / `ImagePullBackOff`.

The fix was to build the matching tag:

```powershell
docker build -t payment-api:day1 .
```

Then recreate or apply the pod:

```powershell
kubectl delete pod payment-api-pod
kubectl apply -f .\k8s\pod.yaml
kubectl get pods
```

The learning pod was successfully `1/1 Running`. The current Deployment is also healthy: it has two pods, each `1/1 Running` with zero restarts.

## Typical Development Workflow

After changing application code:

```powershell
docker build -t payment-api:day1 .
kubectl apply -f .\k8s\deployment.yaml
kubectl get pods
```

For a real deployment, use a new immutable image tag for every release, such as `payment-api:day2` or a version/commit tag. Update `deployment.yaml` with that exact tag, then apply it. Avoid relying on a changed image behind the same tag because Kubernetes may not pull it again.

To inspect problems, use:

```powershell
kubectl get pods
kubectl describe pod <pod-name>
kubectl logs <pod-name>
kubectl get events --sort-by=.metadata.creationTimestamp
```

## Interview Preparation

### Docker questions

**What is the difference between a Docker image and a container?**

An image is an immutable package or template. A container is a running instance created from that image.

**Why use a multi-stage Docker build for .NET?**

The SDK image is needed to restore and compile the application, but it is larger than necessary at runtime. A multi-stage build copies only the published output into the smaller ASP.NET runtime image, reducing size and attack surface.

**What does `-p 5000:8080` mean?**

It maps host port `5000` to container port `8080`. You browse to `localhost:5000`; Docker forwards traffic to the API inside the container at port `8080`.

**Why use `.dockerignore`?**

It excludes unnecessary files from the Docker build context. Here it prevents local `bin/` and `obj/` output from being copied into the container build.

### Kubernetes questions

**What is a pod?**

A pod is Kubernetes' smallest deployable unit. It runs one or more tightly related containers that share network and storage context.

**Why use a Deployment instead of creating a pod directly?**

A Deployment maintains the desired number of pod replicas, replaces failed pods, and performs controlled rolling updates. A standalone pod does not provide these management features.

**What is a Service?**

A Service provides a stable network identity and load balances traffic to matching pods using labels. It prevents clients from depending on individual, temporary pod IP addresses.

**What is the difference between `port`, `targetPort`, and `containerPort`?**

`containerPort` documents the port the application uses in its container. `targetPort` is where a Service forwards traffic on the selected pods. `port` is the Service's own port. In this project, traffic flows from Service port `80` to target/container port `8080`.

**What is the difference between readiness and liveness probes?**

Readiness determines whether a pod should receive traffic. Liveness determines whether Kubernetes should restart a container because the application is unhealthy or stuck.

**What caused `ErrImagePull`?**

The manifest requested `payment-api:day1`, but only `payment-api:latest` was available. Kubernetes could not find the requested tag and attempted to pull it. Building or tagging the matching `day1` image resolved it.

## Next Learning Steps

1. Add a `/health` endpoint to the API, correct probe indentation, and reapply the Deployment.
2. Use `kubectl port-forward service/payment-api-service 5000:80` to reach the Service locally without depending on its NodePort.
3. Change `replicas: 2` to `3`, apply the Deployment, and observe Kubernetes create a third pod.
4. Push versioned images to Docker Hub or Azure Container Registry so a remote Kubernetes cluster can pull them.
