import numpy as np
import matplotlib.pyplot as plt

def generate_circular_aperture(N, L, radius):
    """
    生成中心在(0,0)的圆孔，模拟孔径函数
    N: 网格点数（x,y方向均匀取 N 个点）
    L: x,y 平面的物理尺寸（假设为 [-L/2, L/2]）
    radius: 圆孔半径
    返回：孔径函数 (N×N 的数组) 以及网格坐标
    """
    x = np.linspace(-L/2, L/2, N)
    y = np.linspace(-L/2, L/2, N)
    X, Y = np.meshgrid(x, y)
    aperture = np.where(X**2 + Y**2 <= radius**2, 1.0, 0.0)
    return aperture, X, Y

def fresnel_propagation(U0, z, wavelength, dx):
    """
    利用 FFT 计算 Fresnel 衍射
    U0: 入射光场（二维复数数组）
    z: 光场传播距离（正方向）
    wavelength: 波长
    dx: 网格间距（假设 x 和 y 均匀采样）
    返回：传播 z 后的复振幅分布
    """
    N = U0.shape[0]
    k = 2 * np.pi / wavelength

    # 频域坐标
    fx = np.fft.fftfreq(N, d=dx)
    fy = np.fft.fftfreq(N, d=dx)
    FX, FY = np.meshgrid(fx, fy)

    # Fresnel 传递函数（注意：此处为 paraxial 近似形式）
    H = np.exp(1j*k*z) * np.exp(-1j*np.pi*wavelength*z*(FX**2 + FY**2))

    U1 = np.fft.ifft2(np.fft.fft2(U0) * H)
    return U1

# ----------------参数设置-------------------
wavelength = 0.632e-6  # 波长，例如 0.5 微米
# L = 0.02            # 模拟窗口尺寸（单位可以是毫米或任意归一化单位），x,y 范围为 [-L/2, L/2]
N = 256            # 256 x,y 平面的分辨率（建议足够高以获得细节）
radius = 0.0009       # 圆孔半径
num_z = 100       # 沿 z 方向取 10 个采样点
L = 4 * radius

# 为避免 z=0 时积分发散（或初始平面直接就是孔径函数），从一个很小的正距离开始
z_min = 0.05
z_max = 0.6       # 总传播距离（正方向）

# 生成孔径函数
aperture, X, Y = generate_circular_aperture(N, L, radius)
dx = L / N       # 空间采样间距

# 用于保存每个 z 位置的光场（这里保存复数数据，如果只需要强度，可取 abs()**2）
field_data = np.zeros((num_z, N, N), dtype=np.complex64)
z_values = np.linspace(z_min, z_max, num_z)

for idx, z in enumerate(z_values):
    U = fresnel_propagation(aperture, z, wavelength, dx)
    field_data[idx] = U

# 可选：计算衍射强度（光强）
intensity_data = np.abs(field_data)**2

normalized_data = intensity_data / intensity_data.max()

# 保存数据为 .npy 文件（Unity 可用插件或者自写加载器读取）
np.save('fresnel_diffraction_intensity_normalized.npy', normalized_data)

# 也可以保存为原始二进制数据，注意记录好数据尺寸和数据类型以便 Unity 中解析
normalized_data.astype(np.float32).tofile('fresnel_diffraction_intensity_normalized.raw')

# # ----------------可视化其中一个 z 平面-------------------
# # 生成 x, y 坐标数组（假设在 generate_circular_aperture 中已经生成）
# x = np.linspace(-L/2, L/2, N)
# y = np.linspace(-L/2, L/2, N)
#
# # 找到 x 和 y 坐标在 [-radius, radius] 内对应的索引
# x_idx = np.where(np.abs(x) <= radius)[0]
# y_idx = np.where(np.abs(y) <= radius)[0]
#
# # 裁剪最后一个 z 平面的数据
# cropped_data = normalized_data[-1][np.ix_(y_idx, x_idx)]
#
# # 可视化裁剪后的数据，extent 对应于实际的物理坐标
# plt.figure(figsize=(6,5))
# plt.imshow(cropped_data, extent=[-radius, radius, -radius, radius],
#            cmap='inferno')
# plt.title(f'Diffraction intensity at z = {z_values[-1]:.2f}')
# plt.xlabel('x')
# plt.ylabel('y')
# plt.colorbar(label='Intensity')
# plt.show()


# 假设 N 为偶数，y=0对应的行索引为 idx_y
idx_y = N // 2
slice_xz = normalized_data[:, idx_y, :]

# 逆时针旋转90度
rotated_slice = np.rot90(slice_xz, k=1)  # k=1 表示旋转90度

plt.rcParams['font.sans-serif'] = ['SimHei']  # 使用支持中文的字体
plt.rcParams['axes.unicode_minus'] = False    # 解决负号显示问题

plt.figure(figsize=(8, 6))
# 注意：旋转后，原来的 x 轴和 z 轴会交换
# 这里我们将 extent 调整为：
#   横轴对应原 z 方向 [z_values[0], z_values[-1]]
#   纵轴对应原 x 方向 [-L/2, L/2]
plt.imshow(rotated_slice, extent=[z_values[0], z_values[-1], -L, L],
           aspect='auto', cmap='RdBu_r')
plt.xlabel('z')   # 经过旋转后，原来的 z 轴变为横轴
plt.ylabel('x')   # 经过旋转后，原来的 x 轴变为纵轴
plt.title(' xoz plane(y=0)')
plt.colorbar(label='Intensity')
plt.show()