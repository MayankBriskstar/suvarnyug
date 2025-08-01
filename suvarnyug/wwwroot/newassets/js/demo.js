"use strict";

const modalShowcase = `
<!-- Demo Showcase -->
<style>
.card-documentation {
	display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
	padding: 16px 25px;
    background: #161B2C;
    color: #fff;
    border-radius: 10px;
    border: 1px solid #ffffff14;
}
</style>
<div class="modal fade" tabindex="-1" id="modalShowcase">
	<div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-fullscreen p-5">
		<div class="modal-content rounded-4">
			<div class="modal-header px-md-5 py-md-4 mt-2 mb-3 shadow-sm border-0">
				<h3 class="h5 fw-extrabold mb-0">Choose Layouts Dashboard</h3>
				<button type="button" class="btn-close me-1" data-bs-dismiss="modal" aria-label="Close"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-6 h-6">
				<path fill-rule="evenodd" d="M5.47 5.47a.75.75 0 011.06 0L12 10.94l5.47-5.47a.75.75 0 111.06 1.06L13.06 12l5.47 5.47a.75.75 0 11-1.06 1.06L12 13.06l-5.47 5.47a.75.75 0 01-1.06-1.06L10.94 12 5.47 6.53a.75.75 0 010-1.06z" clip-rule="evenodd" />
			  </svg>
			  </button>
			</div>
			<div class="modal-body px-md-5">
				<div class="row g-5 pb-5"> 
					<div class="col-md-6 col-lg-4">
						<div class="card-documentation h-100">
							<div class="d-flex align-items-center flex-column justify-content-center text-center">
								<img src="assets/img/kaiadmin/logo_documentation.png" height="60" alt="Read Documentation">
								<div class="docs-info ms-3 mb-4">
									<h6 class="fw-bold mb-0 op-8 mt-1">Need help?</h6>
									<p class="fw-bold mb-0 op-5">Please check our docs</p>
								</div>
							</div>
							<a href="../../documentation/index.html" class="btn btn-primary w-100 mb-3">Documentation</a>
							<a href="https://kaiadmin.themekita.com/" class="btn btn-secondary w-100">Buy Now</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo1/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Classic Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo1.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo2/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									White Classic Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo2.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo3/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Dark Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo3.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo4/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Creative Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo4.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo5/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Trendy Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo5.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo6/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Trendy 2 Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo6.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo7/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Horizontal Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo7.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo8/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Enterprise Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo8.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
					<div class="col-md-6 col-lg-4">
						<div class="preview-showcase shadow-sm">
							<a href="../demo9/index.html" data-kt-href="true" class="preview-thumbnail">
								<h3 class="preview-title">
									Futuristic Dashboard	
								</h3>

								<div class="overflow-hidden">
									<img src="assets/img/kaiadmin/demo9.png" class="w-100 rounded-1 shadow-sm preview-img" data-loaded="true">
								</div>
							</a>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>
<!-- End Demo Showcase -->
`;

// window.addEventListener('load', function(event) {
//   $("body").append(modalShowcase);

//   const myModal = new bootstrap.Modal("#modalShowcase");
//   myModal.show();
// });


// Cicle Chart
Circles.create({
	id:           'task-complete',
	radius:       50,
	value:        80,
	maxValue:     100,
	width:        5,
	text:         function(value){return value + '%';},
	colors:       ['#36a3f7', '#fff'],
	duration:     400,
	wrpClass:     'circles-wrp',
	textClass:    'circles-text',
	styleWrapper: true,
	styleText:    true
})

//Notify
//$.notify({
//	icon: 'icon-bell',
//	title: 'Kaiadmin',
//	message: 'Premium Bootstrap 5 Admin Dashboard',
//},{
//	type: 'secondary',
//	placement: {
//		from: "bottom",
//		align: "right"
//	},
//	time: 1000,
//});

// Jsvectormap
//Chart
//const yearlyUserData = @Html.Raw(jsonUserData);

const monthLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
const colors = ['#f3545d', '#fdaf4b', '#177dff', '#00cae3', '#31ce36', '#8e44ad'];

// Function to create datasets for any data
function createChartData(yearlyData) {
	return Object.keys(yearlyData).map((year, index) => {
		const color = colors[index % colors.length];
		return {
			label: `${year}`,
			borderColor: color,
			backgroundColor: color + '66',
			pointBackgroundColor: color + '99',
			pointRadius: 0,
			fill: true,
			borderWidth: 2,
			legendColor: color,
			tension: 0.4,
			data: yearlyData[year]
		};
	});
}

// Common chart options with legend callback
function chartOptions(legendElementId) {
	return {
		responsive: true,
		maintainAspectRatio: false,
		interaction: {
			mode: 'index',
			intersect: false,
		},
		tooltips: {
			bodySpacing: 4,
			mode: "nearest",
			intersect: 0,
			position: "nearest",
			xPadding: 10,
			yPadding: 10,
			caretPadding: 10
		},
		layout: {
			padding: { left: 5, right: 5, top: 15, bottom: 15 }
		},
		scales: {
			y: {  // Updated from yAxes to y
				ticks: {
					fontStyle: "500",
					beginAtZero: false,
					maxTicksLimit: 5,
					padding: 10
				},
				grid: {  // Updated from gridLines to grid
					drawTicks: false,
					display: false
				}
			},
			x: {  // Updated from xAxes to x
				grid: {  // Updated from gridLines to grid
					zeroLineColor: "transparent"
				},
				ticks: {
					padding: 10,
					fontStyle: "500"
				}
			}
		},
		legend: {
			display: false
		},
		legendCallback: function (chart) {
			var text = [];
			text.push('<ul class="' + chart.id + '-legend html-legend">');
			for (var i = 0; i < chart.data.datasets.length; i++) {
				text.push('<li><span style="background-color:' + chart.data.datasets[i].legendColor + '"></span>');
				if (chart.data.datasets[i].label) {
					text.push(chart.data.datasets[i].label);
				}
				text.push('</li>');
			}
			text.push('</ul>');
			document.getElementById(legendElementId).innerHTML = text.join('');
		}
	};
}

// Chart for Users
const ctxUsers = document.getElementById('usersChart').getContext('2d');
const usersChart = new Chart(ctxUsers, {
	type: 'line',
	data: {
		labels: monthLabels,
		datasets: createChartData(yearlyUserData)
	},
	options: chartOptions('myUsersChartLegend')
});

// Chart for Biodata
const ctxBiodata = document.getElementById('biodataChart').getContext('2d');
const biodataChart = new Chart(ctxBiodata, {
	type: 'line',
	data: {
		labels: monthLabels,
		datasets: createChartData(yearlyBiodataData)
	},
	options: chartOptions('myBiodataChartLegend')
});

// Chart for Forums
const ctxForums = document.getElementById('forumsChart').getContext('2d');
const forumsChart = new Chart(ctxForums, {
	type: 'line',
	data: {
		labels: monthLabels,
		datasets: createChartData(yearlyForumsData)
	},
	options: chartOptions('myForumsChartLegend')
});

// Chart for Subscribed Users
const ctxSubscribedUsers = document.getElementById('subscribedUsersChart').getContext('2d');
const subscribedUsersChart = new Chart(ctxSubscribedUsers, {
	type: 'line',
	data: {
		labels: monthLabels,
		datasets: createChartData(yearlySubscribedUsersData)
	},
	options: chartOptions('mySubscribedUsersChartLegend')
});


$("#activeUsersChart").sparkline([112,109,120,107,110,85,87,90,102,109,120,99,110,85,87,94], {
	type: 'bar',
	height: '100',
	barWidth: 9,
	barSpacing: 10,
	barColor: 'rgba(255,255,255,.3)'
});
