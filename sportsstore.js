// sportsstore.js (hoặc file JS riêng cho trang quản trị)
// Hàm để lấy JWT (giả định bạn đã có cơ chế đăng nhập và lưu token vào localStorage)
function getJwtToken() {
    return localStorage.getItem('jwt_token');
}
// Hàm hiển thị danh sách sản phẩm
async function loadProducts() {
    const token = getJwtToken();
    try {
        const response = await fetch('/api/admin/productsapi', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const products = await response.json();
        const productListDiv = document.getElementById('productList');
        productListDiv.innerHTML = ''; // Xóa nội dung cũ
        products.forEach(product => {
            const productItem = document.createElement('div');
            productItem.innerHTML = ` <p>ID: ${product.productID}, Tên: ${product.name}, Giá: ${product.price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })}</p>
            <button onclick="deleteProduct(${product.productID})" class="btn btn-danger btn-sm">Xóa</button>`;
            productListDiv.appendChild(productItem);
        });
    } catch (error) {
        console.error("Lỗi khi tải sản phẩm:", error);
        alert("Không thể tải danh sách sản phẩm. Vui lòng đăng nhập lạihoặc kiểm tra quyền.");
    }
}
// Hàm xóa sản phẩm
async function deleteProduct(productId) {
    if (!confirm('Bạn có chắc chắn muốn xóa sản phẩm này không?')) {
        return;
    }
    const token = getJwtToken();
    try {
        const response = await
            fetch(`/api/admin/productsapi/${productId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
        if (response.status === 204) { // 204 No Content là thành công cho DELETE
            alert('Sản phẩm đã được xóa thành công!');
            loadProducts(); // Tải lại danh sách
        } else if (response.status === 404) {
            alert('Sản phẩm không tìm thấy.');
        } else {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error("Lỗi khi xóa sản phẩm:", error);
        alert("Không thể xóa sản phẩm. Vui lòng thử lại.");
    }
}
// Gọi hàm tải sản phẩm khi trang được load
document.addEventListener('DOMContentLoaded', loadProducts);
/* // Yêu cầu jQuery library
$.ajax({
url: '/api/admin/productsapi',
method: 'GET',
headers: {
'Authorization': `Bearer ${getJwtToken()}`
},
success: function(data) {
// Xử lý dữ liệu trả về
},
error: function(xhr, status, error) {
// Xử lý lỗi
}
});
*/