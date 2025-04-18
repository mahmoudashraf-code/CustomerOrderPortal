let currentCustomer = null;
let token = null;
let banCheckInterval = null;

// Loading indicator functions
function showLoading() {
    document.getElementById('loading-overlay').classList.remove('hidden');
}

function hideLoading() {
    document.getElementById('loading-overlay').classList.add('hidden');
}

// Tab switching functionality
document.getElementById('login-tab').addEventListener('click', function () {
    showTab('login');
});

document.getElementById('register-tab').addEventListener('click', function () {
    showTab('register');
});

function showTab(tabName) {
    // Hide all content
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.add('hidden');
        tab.classList.remove('block');
    });

    // Remove active class from all tabs
    document.querySelectorAll('#auth-tabs button').forEach(button => {
        button.classList.remove('active-tab', 'border-blue-500', 'text-blue-600');
        button.classList.add('border-transparent', 'hover:text-gray-600', 'hover:border-gray-300');
    });

    // Show selected content
    document.getElementById(`${tabName}-content`).classList.remove('hidden');
    document.getElementById(`${tabName}-content`).classList.add('block');

    // Mark selected tab as active
    document.getElementById(`${tabName}-tab`).classList.add('active-tab', 'border-blue-500', 'text-blue-600');
    document.getElementById(`${tabName}-tab`).classList.remove('border-transparent', 'hover:text-gray-600', 'hover:border-gray-300');
}

// Check for existing token on page load
document.addEventListener('DOMContentLoaded', function () {
    const savedToken = localStorage.getItem('token');
    const savedCustomer = localStorage.getItem('customer');

    if (savedToken && savedCustomer) {
        try {
            token = savedToken;
            currentCustomer = JSON.parse(savedCustomer);

            // Show header and set username
            document.getElementById('main-header').classList.remove('hidden');
            document.getElementById('header-username').textContent = currentCustomer.name;

            // Show order sections
            document.getElementById('customer-section').style.display = 'none';
            document.getElementById('order-section').style.display = 'block';
            document.getElementById('view-orders-section').style.display = 'block';
            document.getElementById('delete-order-section').style.display = 'block';

            // Fetch orders and check ban status
            fetchOrders();
            checkBanStatus();

            // Set up periodic ban status checks every minute
            setupBanStatusCheck();
        } catch (error) {
            console.error('Error parsing stored customer data:', error);
            // Clear invalid data
            localStorage.removeItem('token');
            localStorage.removeItem('customer');
        }
    }
});

// Register form handling
document.getElementById('register-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    const name = document.getElementById('customer-name').value.trim();
    const email = document.getElementById('customer-email').value.trim();
    const password = document.getElementById('customer-password').value.trim();

    if (name && email && password) {
        try {
            showLoading();
            const response = await fetch('/api/Customer', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name, email, password })
            });

            if (response.ok) {
                const customer = await response.json();
                alert(`Welcome, ${customer.name}! You can now login.`);

                // Switch to login tab after successful registration
                showTab('login');

                // Pre-fill login form
                document.getElementById('login-email').value = email;
                document.getElementById('login-password').value = password;
            } else {
                alert('Registration failed. Email may be already in use.');
            }
        } catch (error) {
            console.error('Error:', error);
        } finally {
            hideLoading();
        }
    }
});

// Login form handling
document.getElementById('login-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value.trim();

    if (email && password) {
        await loginCustomer(email, password);
    }
});

async function loginCustomer(email, password) {
    try {
        showLoading();
        const response = await fetch('/api/Customer/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (response.ok) {
            const data = await response.json();
            currentCustomer = data;
            token = data.token;

            // Store in localStorage
            localStorage.setItem('token', token);
            localStorage.setItem('customer', JSON.stringify(data));

            // Show header and set username
            document.getElementById('main-header').classList.remove('hidden');
            document.getElementById('header-username').textContent = data.name;

            document.getElementById('customer-section').style.display = 'none';
            document.getElementById('order-section').style.display = 'block';
            document.getElementById('view-orders-section').style.display = 'block';
            document.getElementById('delete-order-section').style.display = 'block';
            fetchOrders();

            // Check ban status
            checkBanStatus();
            setupBanStatusCheck();
        } else {
            alert('Login failed. Check your credentials.');
        }
    } catch (error) {
        console.error('Error:', error);
    } finally {
        hideLoading();
    }
}

document.getElementById('order-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    const orderDetails = document.getElementById('order-details').value.trim();

    if (orderDetails && currentCustomer) {
        try {
            showLoading();
            const response = await fetch('/api/Order', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${token}`
                },
                body: JSON.stringify({
                    customerId: currentCustomer.id,
                    items: [{ productName: orderDetails, quantity: 1, unitPrice: 0 }]
                })
            });

            if (response.ok) {
                document.getElementById('order-details').value = '';
                fetchOrders();
            } else {
                if (response.status === 400) {
                    const errorData = await response.json();
                    if (errorData && typeof errorData === 'string' && errorData.toLowerCase().includes("banned")) {
                        alert("You are temporarily banned from placing orders due to excessive cancellations.");
                        checkBanStatus(); // Refresh ban status to get exact time
                    } else {
                        alert(errorData || 'Failed to place order.');
                    }
                } else {
                    alert('Failed to place order.');
                }
            }
        } catch (error) {
            console.error('Error:', error);
        } finally {
            hideLoading();
        }
    }
});

document.getElementById('delete-order-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    const orderId = parseInt(document.getElementById('order-id').value, 10);

    if (orderId && currentCustomer) {
        try {
            showLoading();
            const response = await fetch(`/api/Order/${orderId}/customer/${currentCustomer.id}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${token}` }
            });

            if (response.ok) {
                alert('Order deleted successfully!');
                document.getElementById('order-id').value = '';
                fetchOrders();

                // Check ban status after deletion
                setTimeout(checkBanStatus, 500); // Small delay to ensure server has processed the deletion
            } else {
                alert('Failed to delete order.');
            }
        } catch (error) {
            console.error('Error:', error);
        } finally {
            hideLoading();
        }
    }
});

async function fetchOrders() {
    try {
        showLoading();
        const response = await fetch(`/api/Order/customer/${currentCustomer.id}`, {
            headers: { Authorization: `Bearer ${token}` }
        });

        if (response.ok) {
            const orders = await response.json();
            renderOrders(orders);
        } else {
            // If we get an unauthorized response, token might be expired
            if (response.status === 401) {
                logout();
                alert('Your session has expired. Please login again.');
            } else {
                alert('Failed to fetch orders.');
            }
        }
    } catch (error) {
        console.error('Error:', error);
    } finally {
        hideLoading();
    }
}

function renderOrders(orders) {
    const ordersList = document.getElementById('orders-list');
    ordersList.innerHTML = `
        <table class="table-auto w-full border-collapse border border-gray-300">
            <thead>
                <tr class="bg-gray-200">
                    <th class="border border-gray-300 px-4 py-2">Order ID</th>
                    <th class="border border-gray-300 px-4 py-2">Details</th>
                    <th class="border border-gray-300 px-4 py-2">Actions</th>
                </tr>
            </thead>
            <tbody>
                ${orders.map(order => `
                    <tr>
                        <td class="border border-gray-300 px-4 py-2 text-center">${order.id}</td>
                        <td class="border border-gray-300 px-4 py-2">${order.items.map(i => i.productName).join(', ')}</td>
                        <td class="border border-gray-300 px-4 py-2 text-center">
                            <button class="delete-order-btn text-red-500 hover:text-red-700" data-orderid="${order.id}">
                                <i class="fas fa-trash-alt"></i>
                            </button>
                        </td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;

    // Add event listeners to delete buttons
    document.querySelectorAll('.delete-order-btn').forEach(button => {
        button.addEventListener('click', handleOrderDelete);
    });
}

// Function to handle order deletion from the table
async function handleOrderDelete(e) {
    if (!confirm('Are you sure you want to delete this order?')) {
        return;
    }

    const orderId = parseInt(e.currentTarget.dataset.orderid, 10);

    if (orderId && currentCustomer) {
        try {
            showLoading();
            const response = await fetch(`/api/Order/${orderId}/customer/${currentCustomer.id}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${token}` }
            });

            if (response.ok) {
                fetchOrders();

                // Check ban status after deletion
                setTimeout(checkBanStatus, 500); // Small delay to ensure server has processed the deletion
            } else {
                alert('Failed to delete order.');
            }
        } catch (error) {
            console.error('Error:', error);
        } finally {
            hideLoading();
        }
    }
}

async function checkBanStatus() {
    if (!currentCustomer || !token) return;

    try {
        showLoading();
        const response = await fetch(`/api/Customer/ban-status`, {
            headers: { Authorization: `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();

            if (data.isBanned && data.banDetails) {
                // Show ban notification
                const banNotification = document.getElementById('ban-notification');
                banNotification.classList.remove('hidden');

                // Disable order form
                const orderForm = document.getElementById('order-form');
                const placeOrderBtn = document.getElementById('place-order-btn');
                placeOrderBtn.disabled = true;
                placeOrderBtn.classList.add('opacity-50', 'cursor-not-allowed');
                placeOrderBtn.classList.remove('hover:bg-green-600');

                // Format ban expiry time
                const expiryDate = new Date(data.banDetails.banEndTime);
                document.getElementById('ban-expiry').textContent = formatDate(expiryDate);

                // Set up countdown
                updateBanCountdown(expiryDate);
            } else {
                // No ban, enable order form
                const banNotification = document.getElementById('ban-notification');
                banNotification.classList.add('hidden');

                const placeOrderBtn = document.getElementById('place-order-btn');
                placeOrderBtn.disabled = false;
                placeOrderBtn.classList.remove('opacity-50', 'cursor-not-allowed');
                placeOrderBtn.classList.add('hover:bg-green-600');
            }
        } else if (response.status === 401) {
            logout();
            alert('Your session has expired. Please login again.');
        }
    } catch (error) {
        console.error('Error checking ban status:', error);
    } finally {
        hideLoading();
    }
}

function setupBanStatusCheck() {
    // Clear any existing interval
    if (banCheckInterval) {
        clearInterval(banCheckInterval);
    }

    // Check ban status every minute
    banCheckInterval = setInterval(checkBanStatus, 60000);
}

function updateBanCountdown(endTime) {
    const countdownElement = document.getElementById('ban-countdown');
    const updateTimer = () => {
        const now = new Date();
        const remaining = endTime - now;

        if (remaining <= 0) {
            countdownElement.textContent = 'Ban expired! You can place orders now.';
            checkBanStatus(); // Refresh the ban status
            return;
        }

        // Calculate hours, minutes, seconds
        const hours = Math.floor(remaining / (60 * 60 * 1000));
        const minutes = Math.floor((remaining % (60 * 60 * 1000)) / (60 * 1000));
        const seconds = Math.floor((remaining % (60 * 1000)) / 1000);

        countdownElement.textContent =
            `${hours}h ${minutes}m ${seconds}s`;
    };

    // Update immediately, then set interval
    updateTimer();
    const countdownInterval = setInterval(updateTimer, 1000);

    // Clear interval when time is up
    setTimeout(() => {
        clearInterval(countdownInterval);
        checkBanStatus();
    }, endTime - new Date());
}

function formatDate(date) {
    const options = {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    return date.toLocaleString(undefined, options);
}

function logout() {
    currentCustomer = null;
    token = null;

    if (banCheckInterval) {
        clearInterval(banCheckInterval);
        banCheckInterval = null;
    }

    localStorage.removeItem('token');
    localStorage.removeItem('customer');

    // Hide header on logout
    document.getElementById('main-header').classList.add('hidden');

    document.getElementById('customer-section').style.display = 'block';
    document.getElementById('order-section').style.display = 'none';
    document.getElementById('view-orders-section').style.display = 'none';
    document.getElementById('delete-order-section').style.display = 'none';
    showTab('login');
}

// Add event listener to the header logout button
document.getElementById('logout-btn').addEventListener('click', logout);
