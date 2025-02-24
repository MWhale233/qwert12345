import numpy as np
import matplotlib.pyplot as plt

def generate_horizontal_slit(N, L, slit_height):
    """
    生成水平单缝孔径函数（单缝横向，即在 y 方向上窄，x 方向全通过）
    N: 网格点数（x,y方向均匀取 N 个点）
    L: x,y 平面的物理尺寸（假设为 [-L/2, L/2]）
    slit_height: 单缝高度（在 y 方向上的宽度）
    返回：孔径函数 (N×N 的数组) 以及网格坐标
    """
    x = np.linspace(-L/2, L/2, N)
    y = np.linspace(-L/2, L/2, N)
    X, Y = np.meshgrid(x, y)
    # 水平单缝：只有当 |Y| <= slit_height/2 时，透光；x方向全透
    aperture = np.where(np.abs(Y) <= slit_height/2, 1.0, 0.0)
    return aperture, X, Y

def fresnel_propagation(U0, z, wavelength, dx):
    """
    利用 FFT 计算 Fresnel 衍射（Fresnel近似）
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

    # Fresnel 传递函数（抛物线近似）
    H = np.exp(1j * k * z) * np.exp(-1j * np.pi * wavelength * z * (FX**2 + FY**2))

    U1 = np.fft.ifft2(np.fft.fft2(U0) * H)
    return U1

# ----------------参数设置-------------------
wavelength = 0.632e-6   # 波长，单位：米
N = 256                 # x,y 平面分辨率
slit_height = 0.0004    # 单缝高度（例如 0.4 mm）
L = 4 * slit_height     # 模拟窗口尺寸，根据需要调整（这里设为单缝高度的4倍）
num_z = 100             # 沿 z 方向采样点数

# 为避免 z=0 时积分发散，从一个很小的正距离开始
z_min = 0.05            # 最小传播距离（单位：米）
z_max = 0.6             # 最大传播距离（单位：米）

# 生成水平单缝孔径函数
aperture, X, Y = generate_horizontal_slit(N, L, slit_height)
dx = L / N            # 空间采样间距

# 用于保存每个 z 位置的光场（这里保存复数数据，如果只需要光强可取 abs()**2）
field_data = np.zeros((num_z, N, N), dtype=np.complex64)
z_values = np.linspace(z_min, z_max, num_z)

for idx, z in enumerate(z_values):
    U = fresnel_propagation(aperture, z, wavelength, dx)
    field_data[idx] = U

# 计算衍射光强（光强 = 振幅的模方）
intensity_data = np.abs(field_data)**2

# 归一化数据
normalized_data = intensity_data / intensity_data.max()

# 保存数据为 .npy 文件（Unity 中可以用插件或自写加载器读取）
np.save('slit_diffraction_single_slit_normalized.npy', normalized_data)

# 也保存为原始二进制数据，便于 Unity 中解析
normalized_data.astype(np.float32).tofile('slit_diffraction_single_slit_normalized.raw')

# ----------------可视化其中一个 z 平面-------------------
plt.figure(figsize=(6,5))
plt.imshow(normalized_data[-1], extent=[-L/2, L/2, -L/2, L/2], cmap='inferno')
plt.title(f'Single Slit Diffraction at z = {z_values[-1]:.2f} m')
plt.xlabel('x (m)')
plt.ylabel('y (m)')
plt.colorbar(label='Intensity')
plt.show()


# ----------------可视化 xoz 平面（假设 y=0 对应的行）-------------------
idx_y = N // 2
slice_xz = normalized_data[:, idx_y, :]

# 逆时针旋转90度，使得 xoz 平面显示更直观（横轴 z，纵轴 x）
rotated_slice = np.rot90(slice_xz, k=1)

plt.figure(figsize=(8, 6))
# 横轴对应原 z 方向，纵轴对应原 x 方向
plt.imshow(rotated_slice, extent=[z_values[0], z_values[-1], -L/2, L/2],
           aspect='auto', cmap='RdBu_r')
plt.xlabel('z (m)')
plt.ylabel('x (m)')
plt.title('xoz plane (y=0) for Single Slit')
plt.colorbar(label='Intensity')
plt.show()
