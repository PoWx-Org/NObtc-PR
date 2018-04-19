﻿using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NBitcoin.Altcoins
{
	// Reference: https://github.com/polispay/polis/blob/master/src/chainparams.cpp
	public class Polis
	{
		public class PolisConsensusFactory : ConsensusFactory
		{
			public PolisConsensusFactory()
			{
			}

			public override BlockHeader CreateBlockHeader()
			{
				return new PolisBlockHeader();
			}
			public override Block CreateBlock()
			{
				return new PolisBlock(new PolisBlockHeader());
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public class PolisBlockHeader : BlockHeader
		{
			// https://github.com/polispay/polis/blob/e596762ca22d703a79c6880a9d3edb1c7c972fd3/src/primitives/block.cpp#L13
			public override uint256 GetHash()
			{
				var headerBytes = this.ToBytes();
				var h = new HashX11.X11().ComputeBytes(headerBytes);
				return new uint256(h);
			}
		}

		public class PolisBlock : Block
		{
#pragma warning disable CS0612 // Type or member is obsolete
			public PolisBlock(PolisBlockHeader h) : base(h)
#pragma warning restore CS0612 // Type or member is obsolete
			{

			}
			public override ConsensusFactory GetConsensusFactory()
			{
				return Polis.Mainnet.Consensus.ConsensusFactory;
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete

		[Obsolete("Use EnsureRegistered instead")]
		public static void Register()
		{
			EnsureRegistered();
		}
		public static void EnsureRegistered()
		{
			if(_LazyRegistered.IsValueCreated)
				return;
			// This will cause RegisterLazy to evaluate
			new Lazy<object>[] { _LazyRegistered }.Select(o => o.Value != null).ToList();
		}
		static Lazy<object> _LazyRegistered = new Lazy<object>(RegisterLazy, false);

		public static Network GetNetwork(NetworkType networkType)
		{
			EnsureRegistered();
			switch (networkType)
			{
				case NetworkType.Main:
					return _Mainnet;
				case NetworkType.Testnet:
					return _Testnet;
				case NetworkType.Regtest:
					return _Regtest;
			}
			return null;
		}
		
		private static object RegisterLazy()
		{
			_Mainnet = mainnetReg();
			_Testnet = testnetReg();
			_Regtest = regtestReg();

			return new object();
		}

		private static Network mainnetReg()
		{
			var builder = new NetworkBuilder();
			var res = builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 1569325056,
				MajorityEnforceBlockUpgrade = 750,
				MajorityRejectBlockOutdated = 950,
				MajorityWindow = 1000,
				BIP34Hash = new uint256("0x000001f35e70f7c5705f64c6c5cc3dea9449e74d5b5c7cf74dad1bcca14a8012"),
				PowLimit = new Target(new uint256("0x00000fffff000000000000000000000000000000000000000000000000000000")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000e72e9b868ce8efb7"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
				PowAllowMinDifficultyBlocks = false,
				CoinbaseMaturity = 15,
				PowNoRetargeting = false,
				RuleChangeActivationThreshold = 1916,
				MinerConfirmationWindow = 2016,
				HashGenesisBlock = new uint256("0x000009701eb781a8113b1af1d814e2f060f6408a2c990db291bc5108a1345c1e"),
				ConsensusFactory = new PolisConsensusFactory(),
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 55 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 56 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 60 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x88, 0xB2, 0x1E })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("polis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("polis"))
			.SetMagic(0xBD6B0CBF)
			.SetPort(24126)
			.SetRPCPort(24127)
			.SetMaxP2PVersion(70208)
			.SetName("polis-main")
			.AddAlias("polis-mainnet")
			.AddDNSSeeds(new[]
			{
				new DNSSeedData("node1.polispay.org", "node1.polispay.org"),
				new DNSSeedData("polis.seeds.mn.zone", "polis.seeds.mn.zone"),
				new DNSSeedData("polis.mnseeds.com", "polis.mnseeds.com"),
				new DNSSeedData("node2.polispay.org", "node2.polispay.org")
			})
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000fc4b8cb903aed54e11e1ae8a5b7ad097ade34988a84500ad2d80e4d1f5bcc95d2bb73b5af0ff0f1edbff04000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff2404ffff001d01041c506f6c69732c2066726f6d2070656f706c6520746f2070656f706c65ffffffff0100f2052a01000000434104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac00000000")
			.SetNetworkType(NetworkType.Main)
			.BuildAndRegister();

			registerDefaultCookiePath(res, ".cookie");

			return res;
		}

		private static void registerDefaultCookiePath(Network network, params string[] subfolders)
		{
			var home = Environment.GetEnvironmentVariable("HOME");
			var localAppData = Environment.GetEnvironmentVariable("APPDATA");
			if(!string.IsNullOrEmpty(home))
			{
				var pathList = new List<string> { home, ".polis" };
				pathList.AddRange(subfolders);

				var fullPath = Path.Combine(pathList.ToArray());
				RPCClient.RegisterDefaultCookiePath(network, fullPath);
			}
			else if(!string.IsNullOrEmpty(localAppData))
			{
				var pathList = new List<string> { localAppData, "Polis" };
				pathList.AddRange(subfolders);

				var fullPath = Path.Combine(pathList.ToArray());
				RPCClient.RegisterDefaultCookiePath(network, fullPath);
			}
		}

		private static Network testnetReg()
		{
			var builder = new NetworkBuilder();
			var res = builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 210240,
				MajorityEnforceBlockUpgrade = 51,
				MajorityRejectBlockOutdated = 75,
				MajorityWindow = 100,
				BIP34Hash = new uint256("0x0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1"),
				PowLimit = new Target(new uint256("0x00000fffff000000000000000000000000000000000000000000000000000000")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
				PowAllowMinDifficultyBlocks = true,
				CoinbaseMaturity = 30,
				PowNoRetargeting = false,
				RuleChangeActivationThreshold = 1512,
				MinerConfirmationWindow = 2016,
				HashGenesisBlock = new uint256("00000bafbc94add76cb75e2ec92894837288a481e5c005f6563d91623bf8bc2c"),
				ConsensusFactory = new PolisConsensusFactory(),
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tpolis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tpolis"))
			.SetMagic(0xFFCAE2CE)
			.SetPort(19999)
			.SetRPCPort(19998)
			.SetMaxP2PVersion(70208)
		   .SetName("polis-test")
		   .AddAlias("polis-testnet")
		   .AddDNSSeeds(new[]
		   {
				new DNSSeedData("polisdot.io",  "testnet-seed.polisdot.io"),
				new DNSSeedData("masternode.io", "test.dnsseed.masternode.io")
		   })
		   .AddSeeds(new NetworkAddress[0])
		   .SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000c762a6567f3cc092f0684bb62b7e00a84890b990f07cc71a6bb58d64b98e02e0dee1e352f0ff0f1ec3c927e60101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff6204ffff001d01044c5957697265642030392f4a616e2f3230313420546865204772616e64204578706572696d656e7420476f6573204c6976653a204f76657273746f636b2e636f6d204973204e6f7720416363657074696e6720426974636f696e73ffffffff0100f2052a010000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000")
		   .SetNetworkType(NetworkType.Testnet)
		   .BuildAndRegister();

			registerDefaultCookiePath(res, "testnet3", ".cookie");

			return res;
		}

		private static Network regtestReg()
		{
			var builder = new NetworkBuilder();
			var res = builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 150,
				MajorityEnforceBlockUpgrade = 750,
				MajorityRejectBlockOutdated = 950,
				MajorityWindow = 1000,
				BIP34Hash = new uint256(),
				PowLimit = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
				PowAllowMinDifficultyBlocks = true,
				CoinbaseMaturity = 30,
				PowNoRetargeting = true,
				RuleChangeActivationThreshold = 108,
				MinerConfirmationWindow = 144,
				HashGenesisBlock = new uint256("000008ca1832a4baf228eb1553c03d3a2c8e02399550dd6ea8d65cec3ef23d2e"),
				ConsensusFactory = new PolisConsensusFactory(),
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tpolis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tpolis"))
			.SetMagic(0xDCB7C1FC)
			.SetPort(19994)
			.SetRPCPort(19993)
			.SetMaxP2PVersion(70208)
			.SetName("polis-reg")
			.AddAlias("polis-regtest")
			.AddDNSSeeds(new DNSSeedData[0])
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000c762a6567f3cc092f0684bb62b7e00a84890b990f07cc71a6bb58d64b98e02e0b9968054ffff7f20ffba10000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff6204ffff001d01044c5957697265642030392f4a616e2f3230313420546865204772616e64204578706572696d656e7420476f6573204c6976653a204f76657273746f636b2e636f6d204973204e6f7720416363657074696e6720426974636f696e73ffffffff0100f2052a010000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000")
			.SetNetworkType(NetworkType.Regtest)
			.BuildAndRegister();

			registerDefaultCookiePath(res, "regtest", ".cookie");

			return res;
		}

		static uint256 GetPoWHash(BlockHeader header)
		{
			var headerBytes = header.ToBytes();
			var h = NBitcoin.Crypto.SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
			return new uint256(h);
		}

		private static Network _Mainnet;
		public static Network Mainnet
		{
			get
			{
				EnsureRegistered();
				return _Mainnet;
			}
		}

		private static Network _Regtest;
		public static Network Regtest
		{
			get
			{
				EnsureRegistered();
				return _Regtest;
			}
		}

		private static Network _Testnet;
		public static Network Testnet
		{
			get
			{
				EnsureRegistered();
				return _Testnet;
			}
		}
	}
}
